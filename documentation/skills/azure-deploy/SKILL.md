---
name: azure-deploy
description: Deploy MyTalli to Azure App Service Linux. Use this skill whenever the user wants to ship a new version, push a build to staging or production, perform a slot swap, debug a failed deploy, or asks "how do I deploy". Also use when the user says "let's deploy", "ship it", "push to prod", "publish", "swap slots", or hits any container start failure (ImagePullFailure, ContainerTimeout, CryptographicException at DataProtection, CredentialUnavailableException). The skill enforces the pre-flight checklist that prevents the v1.0.0.0 deploy disaster from repeating.
---

# Azure Deployment — MyTalli Runbook

This skill covers everything needed to safely deploy MyTalli to Azure App Service Linux. It exists because the **v1.0.0.0 deploy on 2026-05-03 took ~6 hours of debugging** for problems that were entirely preventable with a pre-flight check. Read it before every deploy. No exceptions.

The companion files:
- `memory/feedback_azure_deployment_lessons.md` — the five gotchas with stack traces
- `memory/project_v1_shipped.md` — the current degraded posture and restore path

## When to use this skill

- User says "let's deploy" / "ship it" / "push to prod" / "publish"
- User wants to swap slots
- A deploy just failed and we're in triage
- Container won't start, app returns 500 on every request, App Insights shows weird Azure SDK exceptions
- User asks "how do I deploy"

## Architecture — the boring stuff that bites

- **Hosting:** Azure App Service Linux, plan `mytalli-centralus-asp` (S1 single instance), Central US.
- **App:** `mytalli-web`, runtime stack `Dotnetcore - 10.0`. **`Always on` is enabled** — required for `ShopSyncWorker` + `TokenRefreshWorker` to keep ticking.
- **Slots:** `production` (default, served at `https://www.mytalli.com`) and `staging` (`https://mytalli-web-staging-hsg2aqcrbdc3fqhb.centralus-01.azurewebsites.net`).
- **Custom domain:** `www.mytalli.com` is bound to the production slot only. Domain bindings, TLS certificates, and Managed Identity all stay with the slot during a swap — they do NOT move.
- **App Settings + Connection Strings DO swap with the slot** unless marked "Deployment slot setting" (sticky).
- **Database:** Single Azure SQL DB `MyTalli` on `mytalli-centralus-sql.database.windows.net,1433`. **Both slots point at the same DB.** Anything you do on staging writes to live data.
- **Storage account for DataProtection keys:** `mytallistorage01`, container `dataprotection-keys`, single blob `keys.xml`. Authentication: prod slot's Managed Identity → `Storage Blob Data Contributor` role on the storage account.

## Pre-flight checklist (run BEFORE deploying)

### A. Code is ready
- [ ] All tests pass locally: `dotnet test Source/My.Talli.UnitTesting/My.Talli.UnitTesting.csproj`
- [ ] Version bumped in `Source/My.Talli.Web/My.Talli.Web.csproj` (revision for fixes, patch/minor/major for features). See "One Migration Per Version" rule in CLAUDE.md.
- [ ] All EF migrations consolidated into a single migration file for the version (per CLAUDE.md rule).
- [ ] `appsettings.json` has NO production-only blob storage config that would override env vars on a slot that doesn't have MI.

### B. Database
- [ ] Generate the migration script: `dotnet ef migrations script <last-prod-migration> --project Domain.Data.EntityFramework --startup-project My.Talli.Web --output ../../migrations/prod-upgrade.sql --idempotent`
- [ ] Add the migration ID guard block at the top (see CLAUDE.md "Migration script guard").
- [ ] Run the script against Azure prod DB in SSMS (NEVER `dotnet ef database update --connection ...` against prod).
- [ ] Verify the migration landed: `SELECT TOP 5 MigrationId FROM __EFMigrationsHistory ORDER BY MigrationId DESC;`

### C. Slot configuration parity
- [ ] **Both slots have `WEBSITES_CONTAINER_START_TIME_LIMIT=1800`.** Without this, Linux App Service kills the container after 230 seconds — and .NET 10 cold start exceeds that comfortably.
- [ ] **Both slots have identical app settings + connection strings.** If not, mark the genuinely-different ones as "Deployment slot setting" (sticky) before swapping.
- [ ] **Staging slot has a working Managed Identity** (System-Assigned) AND that identity has `Storage Blob Data Contributor` on `mytallistorage01`. Without it, blob-backed DataProtection breaks on the slot that becomes prod after the swap.
- [ ] **OAuth provider redirect URIs include the staging slot's `*.azurewebsites.net` URL** if you intend to test sign-in directly on staging before swapping. Otherwise sign-in on staging will round-trip to the production custom domain and never come back.

### D. Backup
- [ ] Production app settings + connection strings backed up to a git-ignored folder (e.g., `secrets-backup/`). Use Azure Portal → Production slot → Environment variables → Advanced edit → copy entire JSON. Or `az webapp config appsettings list --resource-group MyTalli-CentralUS-ResourceGroup --name mytalli-web` if your CLI is cooperating.

## Deploy procedure (VS Publish + slot swap)

This is the established path. We use VS Publish ZipDeploy to staging, then swap.

### 1. Publish to staging
- Open `Source/My.Talli.slnx` in Visual Studio.
- Right-click `My.Talli.Web` → Publish.
- **Use the publish profile `mytalli-web-staging - Zip Deploy.pubxml`** (NOT the `Web Deploy` one). Linux App Service does not expose the MSDeploy endpoint (`msdeploy.axd` / Web Management Service) — Web Deploy fails with `ERROR_DESTINATION_NOT_REACHABLE` / `404 Not Found`. The Zip Deploy profile uses Kudu's ZipDeploy API on the SCM URL, which IS available on Linux.
- The Output window should say `Publishing ...\PubTmp\Out\My.Talli.Web-{timestamp}.zip to https://mytalli-web-staging-...scm.centralus-01.azurewebsites.net` — confirm that line before assuming the deploy is happening correctly.
- VS will build, package, ZipDeploy. Takes ~2-5 min.
- **If the publish fails with `MSB4044 missing DestinationUsername`**: Azure Portal → staging slot → Configuration → General settings → enable **SCM Basic Auth Publishing Credentials**. Then in VS, delete all Publish Profiles, re-import a fresh `.PublishSettings` file from Azure, restart VS. **Important:** the import creates a `Web Deploy.pubxml` by default — that one is wrong. Switch to the `Zip Deploy.pubxml` profile (or paste the freshly-downloaded `userPWD` from the `<publishProfile publishMethod="ZipDeploy">` block of the `.PublishSettings` file into the Zip Deploy profile's `.user` file).

### 2. Verify staging is live and serving v{new-version} BEFORE swapping
- Hit `https://mytalli-web-staging-hsg2aqcrbdc3fqhb.centralus-01.azurewebsites.net` and check the version in the footer.
- Hit `/signin` (catches DataProtection + Blazor Server interactive issues — the failure mode that 500'd everything during v1.0.0.0).
- Hit `/privacy` (catches static page rendering issues — usually fine even when dynamic is broken).
- If staging is broken, **DO NOT SWAP**. Triage using the failure-mode section below.

### 3. Swap
- Azure Portal → `mytalli-web` → Deployment slots → **Swap**.
- Source: `staging`, Target: `production`. Confirm.
- Watch the notification bell — typical swap takes 1-3 min, but with v1.0.0.0-class cold starts it can run 8-12 min. Don't panic until 15+.
- **If the swap fails with "did not respond to http ping"**: staging never warmed. Triage staging using failure-mode section, then retry swap.

### 4. Post-swap verification
- `https://www.mytalli.com/` → 200, version footer shows the new version.
- `/signin` → 200.
- Sign in with at least one OAuth provider (Google fastest).
- Walk through `/dashboard`, `/my-plan`, `/platforms`, `/manual-entry`.
- Live Stripe checkout test (cancel out without paying — verifies live keys + no orange TEST MODE banner).
- Live webhook receives a real event.

## Failure-mode triage

When a slot won't start after a deploy, look at **Azure Portal → slot → Instances → Status / Last error column** first. The error will be one of:

### `ImagePullFailure` — `mcr.microsoft.com/appsvc/msitokenservice:stage3`
Azure tried to inject the MSI sidecar container and the image pull failed against MCR. **Not your code's fault — Azure infra issue.** Either wait it out (often clears in 1-4 hours) OR disable System-Assigned Managed Identity on the slot to skip the sidecar entirely (knowing this disables anything that uses MI, including blob-backed DataProtection — see next section).

### `ContainerTimeout — Container did not start within expected time limit`
Three real causes, in order of likelihood:

1. **`WEBSITES_CONTAINER_START_TIME_LIMIT` is missing or low.** Set to `1800`.
2. **DefaultAzureCredential is hanging or throwing during DI startup.** **No longer applicable as of v1.0.0.8** — we removed the `PersistKeysToAzureBlobStorage(blobUri, new DefaultAzureCredential())` call entirely. DataProtection keys now persist to SQL via `PersistKeysToDbContext<TalliDbContext>()`, no Azure credential needed. Historical note: this failure mode is what forced the v1.0.0.0 degraded-posture workaround. Kept here so future-you recognizes the symptom if it ever resurfaces (e.g., if anyone re-adds blob storage).
3. **`DOTNET_STARTUP_HOOKS` injecting `Microsoft.ApplicationInsights.StartupHook.dll` is hanging.** Rare but seen on .NET 10 preview. Workaround: prove it by SSH'ing in (see below) and running with `DOTNET_STARTUP_HOOKS= dotnet My.Talli.Web.dll` to bypass.

### Container appears up but returns 500 on `/` and `/signin` while `/privacy` and `/terms` are 200
You're hitting DataProtection at the `IDataProtector.Protect()` call inside Blazor Server's `ServerComponentSerializer` — the component serializer encrypts component state, fails because DataProtection can't read keys. **Post-v1.0.0.8 the most likely cause is the `components.DataProtectionKey` table being inaccessible** (DB down, connection string wrong, migration didn't run). Verify the table exists and has at least one row. Look at App Insights → Failures → Exceptions → `CryptographicException at Microsoft.AspNetCore.DataProtection`.

## SSH into the container — the fastest debugger

When logs are silent and App Insights is empty, this is the move:

1. Browser → `https://mytalli-web-staging-hsg2aqcrbdc3fqhb.scm.centralus-01.azurewebsites.net/webssh/host` (substitute prod's SCM URL for prod debugging — but be careful, this is the live container).
2. `cd /home/site/wwwroot`
3. `ASPNETCORE_URLS=http://*:9999 dotnet My.Talli.Web.dll 2>&1`

Whatever the dotnet process throws (or hangs on) shows up live in your terminal — no Azure log layer in the way. If you want to bypass the App Insights startup hook to isolate that as a cause, prepend `DOTNET_STARTUP_HOOKS=` (empty value).

## Env vars vs appsettings.json — the precedence trap

`IConfiguration["Foo:Bar"]` reads from environment variables first, then falls through to `appsettings.json`. **Deleting an Azure App Service env var does NOT make `IConfiguration["Foo:Bar"]` return null** if `appsettings.json` has the key.

To force a config value to be empty (e.g., to make a `IsNullOrWhiteSpace` guard trip and skip a code branch), you must explicitly set the env var to a value that satisfies the check. Azure Portal won't accept truly empty strings, so **use a single space `" "`** — `IsNullOrWhiteSpace(" ")` returns true, env vars override `appsettings.json`, the if-block is skipped.

This is exactly what we did to disable blob-backed DataProtection on the prod slot during the v1.0.0.0 deploy. Those single-space env vars are now dead config — v1.0.0.8 stopped reading them. Cleanup: delete them when convenient.

## After the deploy — leftover state to know about

**Post-v1.0.0.8 (2026-05-16) prod posture:** healthy. DataProtection keys persist to `components.DataProtectionKey` (SQL). No MI, no blob, no sidecar dependency.

Cleanup still pending from the v1.0.0.0 → v1.0.0.8 transition:
- `DataProtection__BlobStorage__AccountName` and `__ContainerName` env vars on both slots — set to `" "` from the v1.0.0.0 workaround, v1.0.0.8 no longer reads them. Harmless to leave, tidier to remove.
- `mytallistorage01/dataprotection-keys` blob container — unused. Decommission or keep as cold backup.
- System-Assigned Managed Identity on both slots — currently OFF. v1.0.0.8 doesn't need it. Leave off unless a future feature needs MI for something else (Key Vault, etc.).
- Test platform shops on prod (Etsy + Gumroad) need one manual reconnect: their old ciphertext OAuth tokens (encrypted with the pre-v1.0.0.8 `IShopTokenProtector`) are unreadable to the plaintext-only code path.

## Things this skill MUST prevent us from doing again

1. **Don't enable Managed Identity on a slot without first verifying the MCR sidecar pull works.** When it fails, you can't deploy and the failure mode is opaque. Test in staging first, watch the Activity log for `ImagePullFailure`.
2. **Don't `delete` an env var assuming `IConfiguration` will see null.** Set it to `" "` if you need a guard to skip a branch.
3. **Don't trust "Status: Running" in the Portal.** That only means the container exists, not that your app is responding on port 8080. Always verify by hitting an actual URL.
4. **Don't trust App Insights when the app is silent.** Pre-DI failures are invisible to it. SSH in and run dotnet manually.
5. **Don't deploy without `WEBSITES_CONTAINER_START_TIME_LIMIT=1800` set on both slots.** 230 seconds is not enough for our cold start.
6. **Don't run dev operations against the Azure DB.** Local SQL Server only. Production migrations go through reviewed `.sql` scripts in SSMS.
7. **Don't forget that MI doesn't swap.** Whichever slot is currently serving prod has its OWN MI. Post-swap, the new prod slot has whatever MI configuration its underlying VM had, NOT what you saw in prod before the swap.
