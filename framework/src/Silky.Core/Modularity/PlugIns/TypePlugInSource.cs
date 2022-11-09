using System;
using System.Collections.Generic;
using Silky.Core.Exceptions;
using Silky.Core.Extensions.Collections.Generic;

namespace Silky.Core.Modularity.PlugIns;

public class TypePlugInSource : IPlugInSource
{
    private readonly Type[] _moduleTypes;


    public TypePlugInSource(params string[] moduleTypeNames)
    {
        _moduleTypes = LoadmoduleTypes(moduleTypeNames);
    }

    private Type[] LoadmoduleTypes(params string[] moduleTypeNames)
    {
        var moduleTypes = new List<Type>();
        foreach (var moduleTypeName in moduleTypeNames)
        {
            if (!moduleTypeNames.IsNullOrEmpty())
            {
                var type = Type.GetType(moduleTypeName);
                if (!SilkyModule.IsSilkyModule(type))
                {
                    throw new SilkyException(type.FullName + "is not a module type ");
                }

                moduleTypes.AddIfNotContains(type);
            }
        }

        return moduleTypes.ToArray();
    }


    public Type[] GetModules()
    {
        return _moduleTypes;
    }
}