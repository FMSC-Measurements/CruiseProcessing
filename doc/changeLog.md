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