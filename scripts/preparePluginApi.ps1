$components = Get-ChildItem 'Core/ComponentDefinitions' -Name -Exclude 'IPluginManager.cs' | ForEach-Object { $_.Substring(1, $_.Length - 4) }

$src = 'PluginAPI/Plugin.cs'
$dst = 'PluginAPI/Plugin.cs.pp'
$def = ''
$set = ''

$components | ForEach-Object {
    $def += "        protected I$_ $_ = null;`r`n"
    $set += "            $_ = ComponentManager.GetComponent<I$_>();`r`n"
}

Set-Content $dst ((Get-Content $src -Raw).Replace('        //$$COMPONENTS_DEF', $def).Replace('            //$$COMPONENTS_SET', $set))
