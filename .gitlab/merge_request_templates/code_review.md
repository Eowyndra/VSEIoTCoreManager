### Reviewee

[*"Remember the code isn't you" - Don't take criticism of it personally.*](https://www.dotnetcurry.com/software-gardening/1351/types-of-code-review-benefits "Types of code review benefits")

- [ ] [`Changelog`](CHANGELOG.md) was updated including Gitlab issue id & description (https://keepachangelog.com/en/1.0.0/)  **(Only issues with** ~"W: History"**)**
- [ ] Source code follows our `code style guidelines/coding standards`
- [ ] No warnings or errors from Code analysis (Stylecop; VS)
- [ ] `Logging` messages are meaningful and log level was chosen correctly
- [ ] New `unit tests` were added (if issue requires it)
- [ ] [`README`](README.md) was added/updated (if issue requires it)
- [ ] Current `parent`-branch has been merged into this branch
- [ ] Latest `CI/CD pipeline` was successful (build ok, all unit tests passed)
- [ ] Latest `Test summary` did not worsen (excl. new tests; which must pass!)
- [ ] Move Issue to ~"B2: Review"

*Optional points for reviewee:*

- [ ] [LICENSE.txt](LICENSE.txt) has been updated, **if a new package is used**

### Reviewer

[*"Consider review process of the source code not of the coder."*](https://www.dotnetcurry.com/software-gardening/1351/types-of-code-review-benefits "Types of code review benefits")

- [ ] Can I figure out what the `code` is doing by reading it?
- [ ] Does this code `solve the problem`?
- [ ] Are the `unit tests` useful and understandable?
- [ ] Does the code comply with the `SOLID` priniciples and other `coding basics`?
- [ ] Latest `CI/CD pipeline` was successful (build ok, all unit tests passed)
- [ ] Latest `Test summary` did not worsen (excl. new tests; which must pass!)
- [ ] All items have been checked from `Reviewee`
- [ ] Move Issue to ~"C0: To Be Tested"

_NOTE: Approve code if everything is checked_

### QA-Engineer

- [ ] All tests pass (or previous state didn't worsen)
- [ ] Test coverage did improve (or at least not worsen)
  - Requires approval from QA Lead or PO if coverage declined
- [ ] Are the `unit tests` useful and understandable?
- [ ] Integration tests have been developed and pass
- [ ] Document test result in issue (use [template](https://gitlab-ee.dev.ifm/diagnostic/iot-core/treon/treon_iot_core/-/wikis/30:-Issue-Workflow/Testing) in Wiki)
- [ ] If all checked move issue to ~"C2: Test OK"
- If something **failed**: Move to ~"B0: Sprint" and add ~"H: Test FAILED"

_NOTE: Approve code if everything is checked_
