using System;
using System.Reflection;

namespace TaskExecuter
{
    [Flags]
    public enum JobStatus
    {
        [StringValue("Task Not Started")]
        NotStarted = 0,

        [StringValue("Task Started")]
        Started,

        [StringValue("Task Completed")]
        Completed,

        [StringValue("Task is failed  due to time out.")]
        Timeout,

        [StringValue("Task is cancelled by an user or an external application.")]
        Cancelled,

        [StringValue("Task is failed due to some unhandled exception thrown by an external application.")]
        Failed,

        [StringValue("Invalid task processed by job manager.")]
        InvalidTask
    }

    public static class Extension
    {
        public static string GetStringValue(this Enum context)
        {
            var enumType = context.GetType();
            var enumString = context.ToString();
            FieldInfo fieldInfo = enumType.GetField(enumString);
            if (fieldInfo != null)
            {
                StringValueAttribute[] attributes = fieldInfo.GetCustomAttributes(typeof(StringValueAttribute), false) as StringValueAttribute[];
                if (attributes.Length > 0)
                    return attributes[0].StringValue;
            }

            return enumString;
        }
    }

    /// <summary>
    /// This attribute is used represent a string value for a value in an enum.
    /// </summary>
    public class StringValueAttribute : Attribute
    {
        public string StringValue { get; protected set; }
        public StringValueAttribute(string value)
        {
            this.StringValue = value;
        }
    }
}
