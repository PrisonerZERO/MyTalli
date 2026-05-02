namespace My.Talli.UnitTesting.Services.Admin;

using My.Talli.Web.Services.Admin;

/// <summary>Tests</summary>
public class CircuitTrackerTests
{
    #region <Methods>

    [Fact]
    public void InAppNonAdminCount_DefaultsToZero()
    {
        var tracker = new CircuitTracker();

        Assert.Equal(0, tracker.InAppNonAdminCount);
    }

    [Fact]
    public void Register_NonAdmin_IncrementsCountAndRaisesEvent()
    {
        var tracker = new CircuitTracker();
        var raisedCount = 0;
        tracker.CountChanged += () => raisedCount++;

        tracker.RegisterInAppSession("session-1", isAdmin: false);

        Assert.Equal(1, tracker.InAppNonAdminCount);
        Assert.Equal(1, raisedCount);
    }

    [Fact]
    public void Register_Admin_DoesNotChangeCountOrRaiseEvent()
    {
        var tracker = new CircuitTracker();
        var raisedCount = 0;
        tracker.CountChanged += () => raisedCount++;

        tracker.RegisterInAppSession("admin-1", isAdmin: true);

        Assert.Equal(0, tracker.InAppNonAdminCount);
        Assert.Equal(0, raisedCount);
    }

    [Fact]
    public void Register_DuplicateSessionId_IgnoredOnSecondRegistration()
    {
        var tracker = new CircuitTracker();

        tracker.RegisterInAppSession("session-1", isAdmin: false);
        tracker.RegisterInAppSession("session-1", isAdmin: false);

        Assert.Equal(1, tracker.InAppNonAdminCount);
    }

    [Fact]
    public void Unregister_NonAdmin_DecrementsCountAndRaisesEvent()
    {
        var tracker = new CircuitTracker();
        tracker.RegisterInAppSession("session-1", isAdmin: false);

        var raisedCount = 0;
        tracker.CountChanged += () => raisedCount++;

        tracker.UnregisterInAppSession("session-1");

        Assert.Equal(0, tracker.InAppNonAdminCount);
        Assert.Equal(1, raisedCount);
    }

    [Fact]
    public void Unregister_Admin_DoesNotChangeCountOrRaiseEvent()
    {
        var tracker = new CircuitTracker();
        tracker.RegisterInAppSession("admin-1", isAdmin: true);

        var raisedCount = 0;
        tracker.CountChanged += () => raisedCount++;

        tracker.UnregisterInAppSession("admin-1");

        Assert.Equal(0, tracker.InAppNonAdminCount);
        Assert.Equal(0, raisedCount);
    }

    [Fact]
    public void Unregister_UnknownSession_DoesNothing()
    {
        var tracker = new CircuitTracker();

        var raisedCount = 0;
        tracker.CountChanged += () => raisedCount++;

        tracker.UnregisterInAppSession("does-not-exist");

        Assert.Equal(0, tracker.InAppNonAdminCount);
        Assert.Equal(0, raisedCount);
    }

    [Fact]
    public void MultipleNonAdmins_CountAccumulates()
    {
        var tracker = new CircuitTracker();

        tracker.RegisterInAppSession("s1", isAdmin: false);
        tracker.RegisterInAppSession("s2", isAdmin: false);
        tracker.RegisterInAppSession("s3", isAdmin: false);

        Assert.Equal(3, tracker.InAppNonAdminCount);
    }

    [Fact]
    public void MixedAdminsAndNonAdmins_OnlyNonAdminsCount()
    {
        var tracker = new CircuitTracker();

        tracker.RegisterInAppSession("admin-1", isAdmin: true);
        tracker.RegisterInAppSession("user-1", isAdmin: false);
        tracker.RegisterInAppSession("admin-2", isAdmin: true);
        tracker.RegisterInAppSession("user-2", isAdmin: false);

        Assert.Equal(2, tracker.InAppNonAdminCount);
    }

    [Fact]
    public void EmptySessionId_Ignored()
    {
        var tracker = new CircuitTracker();

        tracker.RegisterInAppSession(string.Empty, isAdmin: false);
        tracker.UnregisterInAppSession(string.Empty);

        Assert.Equal(0, tracker.InAppNonAdminCount);
    }

    #endregion
}
