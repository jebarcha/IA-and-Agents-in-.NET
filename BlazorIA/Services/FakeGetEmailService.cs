using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace BlazorIA.Services;

internal class FakeGetEmailService
{
    [Description("Get a person's email")]
    public string GetEmail([Description("Person's name")] string name) => $"{name}@example.com";
}
