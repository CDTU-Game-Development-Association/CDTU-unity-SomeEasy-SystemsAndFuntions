using System;
using UnityEngine;

namespace CDTU.Utils
{
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class ReadOnlyAttribute : PropertyAttribute
    {
    }
}
