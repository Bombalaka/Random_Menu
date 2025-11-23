using System;

namespace RandomMenuLambda.Models;

public class DeviceRegistration
{
    public string deviceId { get; set; } = string.Empty;
    public string username { get; set; } = string.Empty;
    public DateTime createdAt { get; set; } = DateTime.UtcNow;
    public DateTime lastLogin { get; set; } = DateTime.UtcNow;
}
