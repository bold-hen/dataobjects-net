// Copyright (C) 2025 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Xtensive.Orm.Weaver.Tasks
{
  /// <summary>
  /// Weaves a <see cref="JsonType"/> property setter to call
  /// <c>OnPropertyChanged(propertyName)</c> after setting the backing field.
  /// Unlike Entity property weaving, the backing field is kept intact;
  /// only a notification call is appended to the setter.
  /// </summary>
  internal sealed class ImplementJsonPropertyNotificationTask : WeavingTask
  {
    private readonly TypeDefinition type;
    private readonly PropertyDefinition property;

    public override ActionResult Execute(ProcessorContext context)
    {
      var setter = property.SetMethod;
      if (setter == null)
        return ActionResult.Success;

      var onPropertyChanged = context.References.JsonTypeOnPropertyChanged;

      var il = setter.Body.GetILProcessor();

      // Find the last Ret instruction
      var retInstruction = setter.Body.Instructions.Last(i => i.OpCode == OpCodes.Ret);

      // Insert before Ret:
      //   ldarg.0                          // this
      //   ldstr "PropertyName"             // property name
      //   call JsonType::OnPropertyChanged(string)
      var loadThis = il.Create(OpCodes.Ldarg_0);
      var loadName = il.Create(OpCodes.Ldstr, property.Name);
      var callNotify = il.Create(OpCodes.Call, onPropertyChanged);

      il.InsertBefore(retInstruction, loadThis);
      il.InsertBefore(retInstruction, loadName);
      il.InsertBefore(retInstruction, callNotify);

      return ActionResult.Success;
    }

    public ImplementJsonPropertyNotificationTask(TypeDefinition type, PropertyDefinition property)
    {
      this.type = type ?? throw new ArgumentNullException(nameof(type));
      this.property = property ?? throw new ArgumentNullException(nameof(property));
    }
  }
}
