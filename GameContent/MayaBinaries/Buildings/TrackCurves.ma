//Maya ASCII 2011 scene
//Name: TrackCurves.ma
//Last modified: Tue, Mar 01, 2011 09:53:40 PM
//Codeset: 1252
requires maya "2011";
requires "stereoCamera" "10.0";
currentUnit -l centimeter -a degree -t film;
fileInfo "application" "maya";
fileInfo "product" "Maya 2011";
fileInfo "version" "2011 x64";
fileInfo "cutIdentifier" "201003190311-771506";
fileInfo "osv" "Microsoft Windows 7 Business Edition, 64-bit Windows 7  (Build 7600)\n";
fileInfo "license" "student";
createNode transform -n "TrackCurves:curve1";
createNode nurbsCurve -n "TrackCurves:curveShape1" -p "TrackCurves:curve1";
	setAttr -k off ".v";
	setAttr ".cc" -type "nurbsCurve" 
		1 13 0 no 3
		14 0 0.63831912550000003 1 2 3 4 4.0615165749999997 4.9317064559999997 5 6
		 7 8 8.4115770699999999 9
		14
		-22.410907223736864 -1.4436073823206423 0
		-22.836796530452645 1.7079290267577218 0
		-23.230478794795669 2.9830366596339908 0
		-24.273131868293081 3.2602965343588028 0
		-25.154134527087336 3.0970550796864931 0
		-25.409795940001246 -1.9413342748290867 0
		-20.11370295879755 -2.3412117850902887 0
		21.068373027474415 -3.2433891204360052 0
		27.512116533179093 -3.1344208684273576 0
		27.346866374840772 2.6601534939245219 0
		26.248208443150112 2.5241189483642668 0
		25.370805527991021 1.963990751613486 0
		24.911891334295177 0.7262028554144625 0
		24.652626660172221 -2.8597003091115258 0
		;
createNode transform -n "TrackCurves:curve2";
	setAttr ".t" -type "double3" 0 -1.4343760085956037 0 ;
createNode nurbsCurve -n "TrackCurves:curveShape2" -p "TrackCurves:curve2";
	setAttr -k off ".v";
	setAttr ".cc" -type "nurbsCurve" 
		1 5 0 no 3
		6 0 1 2 3 4 5
		6
		26.319029939580822 0.87094126722396603 0
		17.200439545651193 0.44483891236745166 0
		6.2069987903528725 0.35961844139614474 0
		-3.593355371347192 0.35961844139614474 0
		-13.905032358875083 0.61527985431005172 0
		-23.449725107661234 1.6379255059657156 0
		;
createNode displayLayer -n "CurvesLayer";
	setAttr ".do" 4;
createNode displayLayerManager -n "layerManager";
	setAttr ".cdl" 1;
	setAttr -s 7 ".dli[1:6]"  1 2 3 4 5 6;
	setAttr -s 7 ".dli";
select -ne :time1;
	setAttr ".o" 1;
	setAttr ".unw" 1;
select -ne :renderPartition;
	setAttr -s 32 ".st";
select -ne :initialShadingGroup;
	setAttr -s 25 ".dsm";
	setAttr ".ro" yes;
	setAttr -s 14 ".gn";
select -ne :initialParticleSE;
	setAttr ".ro" yes;
select -ne :defaultShaderList1;
	setAttr -s 11 ".s";
select -ne :defaultTextureList1;
	setAttr -s 12 ".tx";
select -ne :postProcessList1;
	setAttr -s 2 ".p";
select -ne :defaultRenderUtilityList1;
	setAttr -s 17 ".u";
select -ne :renderGlobalsList1;
select -ne :defaultRenderGlobals;
	setAttr ".ren" -type "string" "mayaHardware";
select -ne :defaultResolution;
	setAttr ".pa" 1;
select -ne :hardwareRenderGlobals;
	setAttr ".ctrs" 256;
	setAttr ".btrs" 512;
select -ne :defaultHardwareRenderGlobals;
	setAttr ".fn" -type "string" "im";
	setAttr ".res" -type "string" "ntsc_4d 646 485 1.333";
connectAttr "CurvesLayer.di" "TrackCurves:curve1.do";
connectAttr "CurvesLayer.di" "TrackCurves:curve2.do";
connectAttr "layerManager.dli[4]" "CurvesLayer.id";
// End of TrackCurves.ma
