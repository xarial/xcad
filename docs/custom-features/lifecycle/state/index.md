---
title: Handling the SOLIDWORKS macro feature state update in xCAD framework
caption: State
description: Updating state of the macro feature on the environment change (selection, rebuild, suppress etc.) 
order: 3
---
This handler is called every time state of the feature is changed. It should be used to provide additional security for macro feature.

{% code-snippet { file-name: ~CustomFeature\UpdateStateMacroFeature.* } %}