**Short description of the bug**

[[_TOC_]]

## 0. Meta data

### Device information
|               | Version
| ------        | ------ 
| Device type   | VWV 001
| HW version    | v.X.Y.Z
| SW version    | v.X.Y.Z

### Build version:

1.20.xxx

### Platform:

- Windows 7, 8, 10, Linux etc.
- moneo v.X.Y.Z

## 1. Steps to reproduce:

Detailed step by step command how the bug can be reproduced

e.g.
1. Run Setup.exe
2. Click on "Next"
3. Double click on "Agree"
4. Click on Cancel 
    
## 2. Expected results:

The expected outcome for the correct functioning of the application

## 3. Actual results:

What we found out in the application while testing

## 4. Reproducibility:

Level of consistency: always, intermittent, only once, not reproducible, etc.

## 5. Attachments/Further details:

- Application screenshots
- In case it's an application crash issue, provide a screenshot of the VS debug run call stack **(the single most important thing to add in case of a crash)**
- Application log(s) - useful in case of e.g. device communication issues, visible errors on screen (msg boxes). **Not useful in application crash situations.**
- Other possible details surrounding the situation when the bug occurred

## Definition of ready
<p>
<details>
<summary>Click this to collapse/fold.</summary>

- [ ] Classified/Label issue `(PO)`
   - [ ] Set ~"T1: Bug" or ~"T2: Feature" or ~"T3: Information" or ~"T4: Testing" or similar
   - [ ] (OPTIONAL) Set Priority ~"P1: PATCH" , ~"P2: Hi" , ~"P3: Med" , ~"P4: Lo" or similar
   - [ ] Set ~"W: History" or ~"W: Internal" 
   - [ ] Set ~"Y1: Manual" if relevant for documentation (update/extension)
   - [ ] Set Epic-Label if available
   - [ ] Move to ~"A0: Backlog"
- [ ] Specify issue `PO; DEV; QA`
  - [ ] Furhter specify of the problem and what it solves `(PO)`
  - [ ] Set dependencies to other issues (e.g. blocked by; related; blocking) `(PO)`
  - [ ] Define technical specification `(DEV)`
  - [ ] Define Testcases-Checklist `(QA)`
  - [ ] Define COA-Checklist `(PO)`
  - [ ] Move to ~"A1: Specified"
- [ ] Groom with `team`
  - [ ] Issue is understood?
  - [ ] Testcases are clear?
  - [ ] No missing information for development?
  - [ ] Ready for estimation?
  - [ ] Estimate issue
  - [ ] Update issue description with estimation breakdown
  - [ ] Move to ~"A2: Estimated"
- [ ] Determine feasability `PM/CCB`
  - (>4h estimation) Reject issue if estimation to high/not worth it (~"I1: Do Not Fix")
  - (>4h estimation) Postpone issue if not done in near future (~"I0: Postponed")
  - (> 1 iteration estimation )Split issue into multiple issues
  - [ ] Move to ~"A3: Sprint Ready" if feasable and to be done in near future

</details>
</p>

## Definition of done

<p>
<details open>
<summary>Click this to collapse/fold.</summary>

- [ ] All test case items are checked
- [ ] All COA items are checked
- [ ] Application documentation has been updated/extended (only if ~"Y1: Manual" is set)
- [ ] Code has been merged to it's parent branch 
- [ ] Set ~"P9: SFG" if required
- [ ] Issue is ready to be released (~"E: Completed")

</details>
</p>
