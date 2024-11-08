# Version 11.08.2024

## Fixes
 - Fixed improper stats being shown on ST1 report for plot strata when cruise has multiple plot strata
 - Fixed issue generating reports when value equation has blank grade value
 - Fixed sporadic crashes when generating W2 and W3 reports
 - Fixed warning messages about missing Biomass equations when volume equation has no trees
 - Fixed stratum month and year showing 0 for V3 cruises when tree data has been collected. (excluding trees copied from recon file)
 
## Changes
 - Added error message when multiple volume equations for a species/product exist and calculate the components
 - No longer necessary to go into volume equations page ensure biomass equation info is generated, unless percent removed needs to be changed from default of 95




# Version 09.18.2024

## Fixes
 - Fixed issue where R8 volume equation numbers were being generated with an extra leading '8' 
 - Fixed issue where PDF reports not being created with watermarks
 - Fixed error report showing all reports as selected
 - Fixed calculation of FixCNT biomass values in WT5 report

## Changes
 - Change A06, and A10 reports to display trees in same order as other A reports

# Version 08.19.2024

## Changes
 - R8 and R9 Volume Equations dialog has option to just add missing equations or replace all equations. 
 - Biomass Equations now created for both live and dead if cruise contains both. 
 - Added Live/Dead column to WT1 report. 
 - Add 300FW2W202 equation to R3 Volume Equations

## Fixes
 - Fix Graph report not generating
 - Fix crash when generating PDF and generated graphs are missing

# Version 06.27.2024

## Changes
 - Add warning (to error report or end of cruise) if Volume Equation has Calc Biomass checked 
   but no biomass equations are associated with it.
 - Change error report to include warnings
 - Remove "Incomplete Data" error when opening file. All necessary file checks are performed before processing. 
 - Change Weight Factor use for R1 non-product 01 biomass
 - Add ability to calculate cords to NVB volume equations


## Fixes
 - Fix crash that occurs if opening V3 file that is already open in another instance
 - Fixed Crash that can occur in R9 Vol Equations if species is not an FIA code. 
 - Fixed A01 report displaying BAF/FPS when it doesn't apply to cruise method.
   This can happen if BAF or FPS is set on stratum and then the cruise method is changed. 
 - Fix some reports displaying incorrect heading on fields, if a similar report had been run before it. 
 - Fix crash when modifying volume equations and species missing FIA code or when Tree Default Value FIA code is blank or 0.
   

# Version 05.17.2024

## Fixes
   - Fixed Volume Equations button not showing up after adding R9 equations
   - Fixed volume not being calculated on trees using MerchHeightSecondary
   - Fixed V3 file Biomass Equation not persisting from R8, R8 setup 
   - Fixed V3 file Volume Equations not persisting in V3 file sometimes
   - Fixed editing selected report or using add standard reports clears selected report next time V3 file is opened
   - Fixed contract species being applied to all population with a given product when design has contract species mapped to specific species/product 



# Version 04.23.2024
## Changes
 - Volume Library Updates 
     - Add R6 Equations A16CURW351, NVBM240351
     - Modify R3 Equations 
       - Remove Forest Level Equations
       - Update equations for White fir, White Pine, Engelmann's Spruce, and Quaking Aspen to NVB
       - Remove Douglas Fir equation
## Fixes 
 - Fix issue opening V3 if file has plots missing strata errors


# Version 03.13.2024

## Enhancements
 - Allow opening V3 template files
 - Processing cruises takes significantly less time

## Changes
 - Now if report fails to generate, Cruise Processing wont crash and will continue to generate remaining reports. A error message will indicate which reports failed to generate. 
 - When processing detects errors and displays error report, Cruise Processing no longer exists after closing error report



## Fixes
 - fix BLM01, 02, 09, 10 reports not generating
 - fix VSM5 report not generating when UOM is 05
 - fix issue with VSM4 and VSM5 when no other UC reports selected
 - fix a crash issue when generating CSV files
 - fix validation issues. See Validation Fixes/Changes

### Validation Fixes/Changes
#### Additions 
 - validate Sale District is a number
#### Fixes
 - fix audit on 3ppnt checking plots only contain all count or measure trees
#### Changes
 - 3PPNT when Stratum contains multiple SG, error now shows on the stratum rather than tree records
 - 3PPNT when plot contains mix of count and measure trees, error shows at plot level rather than on tree records
#### Reinstated Validations
 - Tree: UpperStemDiameter greater than DBH
 - Tree: RecoverablePrimary greater than SeenDefectPrimary (except region 10)
 - Tree: TopDIBPrimary greater than TopDIBSecondary
 - Tree: check has height (also validated by FScruiser)
 - Tree: check has Tree Default Value
 - Tree: check has Grade
 - Logs: check tree doesn't have more than 20 logs
 - Logs: if FBS check Net greater than Gross
 - Logs: check seen defect greater than recoverable
 - Logs: check has grade
 - Volume Equation: check each tree species, prod combination has a volume equation
 - Volume Equation: for DVE equations ensure if CalcTopwod set, any of CalcBoard, CalcCubic or CalcCord must be set
 - Volume Equation: for DVE equations ensure no duplicate equations with same TopDIBPrimary value
 - Volume Equation: ensure TopDibSecondary greater than TopDIBPrimary
 - Volume Equation: ensure Equation Number is exactly 10 characters long
 - Volume Equation: ensure no duplicate volume equations
