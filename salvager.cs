/*Pure salvager v1.0
-you have to see on your overview the wrecks ( use my wiki page to see the guide)
-no warp/delete bookmarks availble
-but it recover your mtu and sound a beep when you dont have wrecks on overview
 */
using BotSharp.ToScript.Extension;
using Parse = Sanderling.Parse;
using MemoryStruct = Sanderling.Interface.MemoryStruct;
using System.IO;
using System.Collections.Generic;
using Bib3.Geometrik;
string ModuleSalvagerX = "Salvager";
int salvageRange = 5000;
string MTUName = "put the name of mtu";
Sanderling.Parse.IMemoryMeasurement Measurement => Sanderling?.MemoryMeasurementParsed?.Value;
Sanderling.Parse.IWindowOverview WindowOverview =>
    Measurement?.WindowOverview?.FirstOrDefault();

Parse.IOverviewEntry[] ListWreckOverviewEntry =>
	WindowOverview?.ListView?.Entry
	?.Where(entry => entry.Name.RegexMatchSuccessIgnoreCase("wreck"))
	?.OrderBy(entry => entry.DistanceMax ?? int.MaxValue)
	?.ToArray();

Sanderling.Accumulation.IShipUiModule[] SetModuleSalvager =>
    Sanderling.MemoryMeasurementAccu?.Value?.ShipUiModule?.Where(module => module?.TooltipLast?.Value?.LabelText?.Any(
    label => label?.Text?.RegexMatchSuccess(ModuleSalvagerX, System.Text.RegularExpressions.RegexOptions.IgnoreCase) ?? false) ?? false)?.ToArray();	

Sanderling.Accumulation.IShipUiModule[] SetModuleSalvagerInactive	 =>
	SetModuleSalvager?.Where(module => !(module?.RampActive ?? false))?.ToArray();
	Parse.IOverviewEntry[] listOverviewMtu => WindowOverview?.ListView?.Entry
    ?.Where(mtu =>  (mtu?.Name?.RegexMatchSuccessIgnoreCase(MTUName) ?? true) 
          )
    .ToArray();
    Sanderling.Parse.IShipUiTarget[] SetTargetWreck =>	Measurement?.Target?.Where(target =>
		target?.TextRow?.Any(textRow => textRow.RegexMatchSuccessIgnoreCase("wreck")) ?? false)?.ToArray();
var lockTargetKeyCode = VirtualKeyCode.LCONTROL;
while(true)
{
ModuleMeasureAllTooltip();
if (ListWreckOverviewEntry.Length > 0)
    SalvagingStep();
else 
    {
    Host.Log(" no more wrecks");
    Host.Delay(3111); 
    Console.Beep(1500, 200);
    }
 Host.Delay(7111);   
}

Func<object> SalvagingStep()
{
	
    Host.Log("               Salvaging step");
    var moduleSalvagerInactive = SetModuleSalvagerInactive?.FirstOrDefault();


    if (ListWreckOverviewEntry.Length == 0 || ListWreckOverviewEntry?.FirstOrDefault()?.DistanceMax > salvageRange)
    {

                Host.Log("               salvaging step, time to recover mtu");
        if(listOverviewMtu?.Length >0  )
        {
                    var LootButton = Measurement?.WindowInventory?[0]?.ButtonText?.FirstOrDefault(text => text.Text.RegexMatchSuccessIgnoreCase("Loot All")); 
            
            if (LootButton != null)
                {
                Sanderling.MouseClickLeft(LootButton);
                Host.Log("               loot mtu :))");
                if (listOverviewMtu?.Length >0 )
                    RecoverMtu ();

                }
            if ( listOverviewMtu?.FirstOrDefault()?.DistanceMax > 0)
             {   ClickMenuEntryOnMenuRoot(listOverviewMtu?.FirstOrDefault(), "open cargo"); return SalvagingStep; }
        }
            Host.Log("               no wrecks to salvage with salvagers, Finish the site");
       
 
    }
    if (null == moduleSalvagerInactive)
    {
            Host.Delay(1111);
        return SalvagingStep;
    }
    var setTargetWreckInRange   =
        SetTargetWreck?.Where(target => target?.DistanceMax <= salvageRange)?.ToArray();

        
            var wreckOverviewEntryNextNotTargeted = ListWreckOverviewEntry?.Where(entry => !((entry?.MeTargeted ?? false) || (entry?.MeTargeting ?? false))).ToArray();
    if (wreckOverviewEntryNextNotTargeted?.FirstOrDefault()?.DistanceMax > salvageRange)
    {
            Host.Log("out of range");//to add something if need
        ClickMenuEntryOnMenuRoot(wreckOverviewEntryNextNotTargeted?.FirstOrDefault(), "approach");

    }
    else
    {
    var virtualtargets =(!Measurement?.Target?.IsNullOrEmpty() ?? false ) ? Measurement?.Target?.Length:0;
    Host.Log(" "+Measurement?.Target?.Length+ " ; " +SetModuleSalvagerInactive?.Length + "   ; " +virtualtargets );
    if (  (Measurement?.Target?.IsNullOrEmpty() ?? false) || (virtualtargets <5 && !(ListWreckOverviewEntry?.IsNullOrEmpty() ?? false)))
            for (int i = 0; i < SetModuleSalvagerInactive?.Length ; i++)
           {
           virtualtargets =(!Measurement?.Target?.IsNullOrEmpty() ?? false ) ? Measurement?.Target?.Length:0;
           if ((ListWreckOverviewEntry?.IsNullOrEmpty() ?? false)|| virtualtargets == SetModuleSalvagerInactive?.Length)
           break;
            Sanderling.KeyDown(lockTargetKeyCode);
            Sanderling.MouseClickLeft(wreckOverviewEntryNextNotTargeted.ElementAtOrDefault(i));
            Sanderling.KeyUp(lockTargetKeyCode);  
           continue;
           }
           
    }

    var setTargetWreckInRangeNotAssigned =
        setTargetWreckInRange?.Where(target => !(0 < target?.Assigned?.Length))?.ToArray();
    if(SetModuleSalvagerInactive?.Length >0  && setTargetWreckInRangeNotAssigned?.Length >0)
    {
    
    for (int i = 0; i <SetModuleSalvagerInactive?.Length ; i++)
    {
    setTargetWreckInRangeNotAssigned =
        setTargetWreckInRange?.Where(target => !(0 < target?.Assigned?.Length))?.ToArray();
    if (setTargetWreckInRangeNotAssigned?.IsNullOrEmpty() ?? false)
    break;
    Sanderling.MouseClickLeft(setTargetWreckInRangeNotAssigned?.ElementAtOrDefault(i));
    ModuleToggle(SetModuleSalvagerInactive?.FirstOrDefault());
    
    continue;
    
    }

        return SalvagingStep;
    }
    var wreckOverviewEntryNext = ListWreckOverviewEntry?.FirstOrDefault();


    if(null == wreckOverviewEntryNext || null == wreckOverviewEntryNextNotTargeted)
    {
        Host.Log("   nu mai am wrecks??    ");
        return SalvagingStep;
    }

    return SalvagingStep;
}
void ModuleToggle(Sanderling.Accumulation.IShipUiModule Module)
{
    var ToggleKey = Module?.TooltipLast?.Value?.ToggleKey;
    Host.Log("               Toggle module  '" +Module?.TooltipLast?.Value?.LabelText?.ElementAtOrDefault(1)?.Text?.RemoveXmlTag() +      "'  using " + (null == ToggleKey ? "mouse" : Module?.TooltipLast?.Value?.ToggleKeyTextLabel?.Text));
    if (null == ToggleKey)
        Sanderling.MouseClickLeft(Module);
    else
        Sanderling.KeyboardPressCombined(ToggleKey);
}
void ClickMenuEntryOnMenuRoot(IUIElement MenuRoot, string MenuEntryRegexPattern)
{
    Sanderling.MouseClickRight(MenuRoot);
    var Menu = Measurement?.Menu?.FirstOrDefault();
    var MenuEntry = Menu?.EntryFirstMatchingRegexPattern(MenuEntryRegexPattern, RegexOptions.IgnoreCase);
    Sanderling.MouseClickLeft(MenuEntry);
}
void RecoverMtu ()
{
     if (listOverviewMtu?.Length >0 )
    {
        
        Host.Delay(1111);
        Host.Log("      Recovering Mtu.");
        ClickMenuEntryOnMenuRoot(listOverviewMtu?.FirstOrDefault(), "Scoop to Cargo Hold");
        Host.Delay(1111);       
    }
    
}
void ModuleMeasureAllTooltip()
{
    var moduleUnknownCount = Sanderling.MemoryMeasurementAccu?.Value?.ShipUiModule?.Count((module => null == module?.TooltipLast?.Value?.LabelText?.Any()));
    var initialmoduleCount = moduleUnknownCount;


    while( moduleUnknownCount >0	)
	{
		if(( Measurement?.IsDocked ?? false))
			break;
        for (int i = 0; i < Sanderling.MemoryMeasurementAccu?.Value?.ShipUiModule?.Count(); ++i)
		{
            var NextModule = Sanderling.MemoryMeasurementAccu?.Value?.ShipUiModule?.ElementAtOrDefault(i);

			if(null == NextModule)
				break;
			Sanderling.MouseMove(NextModule);
            Host.Delay(305);
			Sanderling.WaitForMeasurement();

                Host.Delay(305);
			Sanderling.MouseMove(NextModule);

            Host.Log("               R2D2 detected a new module: " +Measurement?.ModuleButtonTooltip?.LabelText?.ElementAtOrDefault(1)?.Text?.RemoveXmlTag() + "");
		}


        moduleUnknownCount = Sanderling.MemoryMeasurementAccu?.Value?.ShipUiModule?.Count((module => null == module?.TooltipLast?.Value?.LabelText?.Any()));
        Host.Log("               R2D2 updated counted modules from " + initialmoduleCount+ " to : " +moduleUnknownCount+"");

    }
}
