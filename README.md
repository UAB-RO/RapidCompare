# UABRO.RapidCompare

The applications here use the Eclipse Scripting Application Programming Interface (ESAPI, Varian Medical Systems) to create a .NET-based application, RapidCompare, that allows for automated validation of RapidPlan models as described in [Harms J, Pogue JA, Cardenas CE, Stanley DN, Cardan R, Popple R. Automated evaluation for rapid implementation of knowledge-based radiotherapy planning models. J Appl Clin Med Phys. 2023 Oct;24(10):e14152. doi: 10.1002/acm2.14152. Epub 2023 Sep 13. PMID: 37703545; PMCID: PMC10562024.](https://doi.org/10.1002/acm2.14152)

This repository contains two applications, RapidPlanComparisonPlanExtractionConsole and RapidPlanEvaluationReportingConsole.

## Applications

### RapidPlanComparisonPlanExtractionConsole

The console application RapidPlanComparisonPlanExtractionConsole automatically creates RapidPlan plans for a specified model based on reference plans. It extracts dosimetric data for the RapidPlan and reference plans and saves the results to a JSON file.

The location of the RapidPlan model database is hard coded in the console application file `Program.cs`. The line

    RapidPlanHelper.PlanningDataBaseServer = "change_to_rapidplan_db_server"; // change this string to the name of the server where the RapidPlan database is located, typically the Aria SQL server.
    
should be changed to the name of the server hosting the RapidPlan model database, which is typically the Aria database server.

#### Input

The input to the automated planning tool is a tab delimited text file. It is comprised of three groups of rows, a section defining the RapidPlan model and calculation options, a single row defining the model structures, and a list of reference plans with the mapping of structure IDs to model structures. The tool copies the reference plan, sets the calculation options, applies the RapidPlan, and then optimizes and calculates the result.

The first section of the input file is a set of keyword-value pairs giving the RapidPlan model ID and the the calculation models to be used for calculation of the RapidPlan. Calculation model strings are given by the Eclipse scripting API CalculationType enumeration member names. Calculation models are optionally followed by calculation option key-value pairs. If a given calculation model is used for multiple calculation types (PhotonIMRTOptimization and PhotonVMATOptimization), the same calculation options will be used. If a calculation model is not specified in the first section, the model and options given in the reference plan will be used. The keywords are summarized below.

| Keyword      | Description of value | Value if missing | Comments |
| ----------- | ----------- | ----------- | ----------- |
| RapidPlanModelID      | ID of the RapidPlan model       | *Required* |
|  PhotonVolumeDose | Photon Volume Dose calculation. | Calculation model of the reference plan | Optionally followed by calculation option key-value pairs |
|  PhotonSRSDose | Photon SRS Dose calculation.  | Calculation model of the reference plan | Optionally followed by calculation option key-value pairs |
|  PhotonOptimization | Photon optimization.  | Calculation model of the reference plan | Optionally followed by calculation option key-value pairs |
|  PhotonIMRTOptimization | Photon IMRT optimization.  | Calculation model of the reference plan | Optionally followed by calculation option key-value pairs |
|  PhotonVMATOptimization | Photon VMAT optimization.  | Calculation model of the reference plan | Optionally followed by calculation option key-value pairs |
|  PhotonLeafMotions | Photon Leaf motion calculation.  | Calculation model of the reference plan | Optionally followed by calculation option key-value pairs |
|  ProtonVolumeDose | Proton Volume Dose calculation.  | Calculation model of the reference plan | Optionally followed by calculation option key-value pairs |
|  DVHEstimation | DVH Estimation.  | Calculation model of the reference plan | Optionally followed by calculation option key-value pairs |
|  ProtonDVHEstimation | Proton DVH Estimation.  | Calculation model of the reference plan | Optionally followed by calculation option key-value pairs |
|  ProtonOptimization | Proton optimization.  | Calculation model of the reference plan | Optionally followed by calculation option key-value pairs |
|  ProtonBeamLineModifiers | Proton beam line modifers.  | Calculation model of the reference plan | Optionally followed by calculation option key-value pairs |
|  ProtonMSPostProcessing | Proton MS post processing.  | Calculation model of the reference plan | Optionally followed by calculation option key-value pairs |
|  ProtonDDC | Proton DDC | Calculation model of the reference plan | Optionally followed by calculation option key-value pairs |


The row following the initial section is the header for the list of reference plans and contains the model structure IDs. The first part of this row must be the following strings separated by tabs: PatientID, ReferenceCourseID, ReferencePlanID, RapidPlanCourseID, and RapidPlanID. The next part of the row is a tab delimited list of model structures. For organs at risk, the model structures should match the structure names in the RapidPlan model. For targets, there should be two tab separted entries, the first being the RapidPlan model ID of the target and the second one starting with Dose[Gy]_ or Dose[cGy]_, followed by the model ID.

Subsequent rows are the patient ID, course ID of the reference plan, ID of the reference plan, course ID for the RapidPlan, ID of the RapidPLan plan, and patient specific matches for the model structures and target prescription doses.

Example

    RapidPlanModelID    UAB Hypofx Prostate+Nodes
    DVHEstimation   DVH Estimation Algorithm [16.1.0]
    PhotonVolumeDose  AcurosXB_1610,CalculationGridSizeInCM,0.1
    PhotonVMATOptimization    PO_1610
    PhotonIMRTOptimization    PO_1610
    PhotonLeafMotions	Varian Leaf Motion Calculator [13.6.23]	
    PatientID   ReferenceCourseID   ReferencePlanID RapidPlanCourseID   RapidPlanID Bladder Bowel_Small    Rectum  PTV_High^p+sv    Dose[cGy]_PTV_High^p+sv PTVn_Low    Dose[cGy]_PTVn_Low
    12345   C1  1.1 Prostate    RapidPlanTest   RP prostate SmBowel rect    PTV70   7000    PTV54   5400

### RapidPlanEvaluationReportingConsole

This console application read ths JSON file created by RapidPlanComparisonPlanExtractionConsole and generates a PDF report.

## External dependencies
The dependencies listed below are for external libraries, and do not include Windows or project dependencies.

###  RapidPlanComparisonPlanExtractionConsole
- **EntityFramework**: Version 6.4.4
- **alglib.net**: Version 3.17.0.0
- **ConsoleX**: Version 1.1.0.0
- **MathNet.Numerics**: Version 4.15.0.0
- **Newtonsoft.Json**: Version 13.0.1
- **NLog**: Version 4.7.10
- **RapidPlanQ**: Version 1.0.0.0
- **VMS.TPS.Common.Model.API**: Version 1.0.450.29
- **VMS.TPS.Common.Model.Types**: Version 1.0.450.29

### UABRO.RapidCompare.Extraction

- **EntityFramework** Version 6.4.4
- **alglib.net** Version 3.17.0.0
- **MathNet.Numerics** Version 4.15.0.0
- **Newtonsoft.Json** Version 13.0.2
- **NLog** Version 4.7.10
- **RapidPlanQ** Version 1.0.0.0
- **System.ValueTuple** Version 4.5.0
- **VMS.TPS.Common.Model.API** Version 1.0.450.29
- **VMS.TPS.Common.Model.Types** Version 1.0.450.29
- 
### UABRO.RapidCompare.Model

- **Newtonsoft.Json** version 13.0.1

### RapidPlanEvaluationReportingConsole

- **ConsoleX**: Version 1.1.0.0

### UABRO.RapidCompare.Analysis

- **alglib.net** Version 3.17.0.0
- **MathNet.Numerics** Version 4.15.0.0

### UABRO.RapidCompare.Reporting

  - **PdfSharp.MigraDoc.Standard** Version 1.51.15
  - **ScottPlot** Version 4.0.48
