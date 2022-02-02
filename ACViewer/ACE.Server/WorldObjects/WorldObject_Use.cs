using ACE.Entity.Enum.Properties;

namespace ACE.Server.WorldObjects
{
    partial class WorldObject
    {
        public double? UseTimestamp
        {
            get => GetProperty(PropertyFloat.UseTimestamp);
            set { if (!value.HasValue) RemoveProperty(PropertyFloat.UseTimestamp); else SetProperty(PropertyFloat.UseTimestamp, value.Value); }
        }

        protected double? ResetTimestamp
        {
            get => GetProperty(PropertyFloat.ResetTimestamp);
            set { if (!value.HasValue) RemoveProperty(PropertyFloat.ResetTimestamp); else SetProperty(PropertyFloat.ResetTimestamp, value.Value); }
        }

        protected double? ResetInterval
        {
            get => GetProperty(PropertyFloat.ResetInterval);
            set { if (!value.HasValue) RemoveProperty(PropertyFloat.ResetInterval); else SetProperty(PropertyFloat.ResetInterval, value.Value); }
        }

        protected bool DefaultLocked
        {
            get => GetProperty(PropertyBool.DefaultLocked) ?? false;
            set { if (!value) RemoveProperty(PropertyBool.DefaultLocked); else SetProperty(PropertyBool.DefaultLocked, value); }
        }

        protected bool DefaultOpen
        {
            get => GetProperty(PropertyBool.DefaultOpen) ?? false;
            set { if (!value) RemoveProperty(PropertyBool.DefaultOpen); else SetProperty(PropertyBool.DefaultOpen, value); }
        }
    }
}
