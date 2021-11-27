using System;
using System.Collections.Generic;
using PowerSchemaFlyout.Models;

namespace PowerSchemaFlyout.Services;

public interface IPowerManagementServices
{
    string GetPowerPlanName(Guid guid);
     List<PowerSchema> GetCurrentSchemas();
     Guid GetActiveGuid();
     void SetActiveGuid(Guid guid);
     event EventHandler ActiveGuidChanged;
    }