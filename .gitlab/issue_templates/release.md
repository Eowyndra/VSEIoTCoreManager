_Naming convention for title:_ `[version] Release Documentation`

**Create documents (RM)**

* [ ] create gitlab issue as **release documentation** from the template issue
* [ ] create **release test protocol** from template 
  - [ ] use the following template: \\intra.ifm\fs\ifort\ift_development\Produkte\[projectname]\PrüfungenAbnahme\Freigabe\[filename]
  - [ ] save the document according to the following format: [yymmdd]_ReleaseTestProtocol_[projectname]_V [x.x.w]-rc_WirdV[x.x.x].docx
* [ ] create **release protocol** 
  - [ ] use the following template: \\intra.ifm\fs\ifort\QS\Freigaben\efe800\Software\TE-DIA-ESW-01_TemplateSoftwareReleaseProtocol.dotx
  - [ ] save the document according to the following format: [yymmdd]_Freigabeprotokoll_[projectname]_V[x.x.x].docx

**Generate and ensure releasable state: (RM)**
* [ ] check if all issues within the milestone are finished and tested
  - [ ] all issues finished and tested
    - [ ] if not: report unfinished issues to project lead
  - [ ] fill in section 2 "change documentation" in the release test protocol
* [ ] check CI pipeline for failures in [release branch] branch
  - [ ] build passed
  - [ ] test passed
  - [ ] test report passed
  - only if applicable: discuss and prepare special release for deficits
* [ ] update change documentation  
  - [ ] update [`Changelog`](CHANGELOG.md) (https://keepachangelog.com/en/1.0.0/)
  - [ ] commit changes
  - [ ] merge changes into [release branch]

**Build release candidate: (RM)**
* [ ] set release candidate version to Vx.x.w
* [ ] start the CI pipeline for the release candidate in the [release branch]
  - [ ] release successful
* [ ] sign the build

**Perform final acceptance tests: (QA)**
* [ ] assign issue to tester for final testing
* [ ] perform release tests
  - [ ] check if [[`documentation`](docs/Treon_IoTCore_Documentation.docx)].docx reflects implemented changes
  - [ ] fill in test protocol
  - [ ] list failed automated test cases
* [ ] evaluate test result
  - [ ] final acceptance tests "passed"
* [ ] copy pdf version of test protocol to \\intra.ifm\fs\ifort\ro\SignaturExchange\SW
* [ ] report bugs into GitLab (if applicable)
* [ ] assign issue to release manager

**Perform release meeting (QA, PO, TM, PM)**
* [ ] copy pdf version of release protocol to \\intra.ifm\fs\ifort\ro\SignaturExchange\SW
* [ ] collect release protocol signatures 
  - only if applicable: collect special release signatures (CEO) 
* [ ] upload release protocol to \\intra.ifm\fs\ifort\QS\Freigaben\efe800\Software
* [ ] delete release protocol from \\intra.ifm\fs\ifort\ro\SignaturExchange\SW
* [ ] release manager communicates release decision to releaser

**Check plausibility of release decision (RM)**
* [ ] check if release protocol is signed
* [ ] check release protocol if all listed special releases are signed (only if applicable)
* [ ] check test protocol if final acceptance tests passed 
* [ ] assign issue to releasing developer

**Perform final release (D)**
* [ ] merge the _release branch_ content into _master_
* [ ] update CI version to Vx.x.x
* [ ] perform final release build in the CI
  - [ ] build passed
  - [ ] test passed
  - [ ] test report passed
* [ ] Fix the new nuget package checksum on [JFrog](https://jfrog.i40.ifm-datalink.net/artifactory/diagnostics-nuget/)
* [ ] Create a new release on the Gitlab [releases](https://gitlab-ee.dev.ifm/diagnostic/iot-core/treon/treon_iot_core/-/releases) page
* [ ] tag the release version in the repository

**Deliver release (RM)**
* [ ] upload the release to \\intra.ifm\FS\ifort\diag_sw_release\Tools\[projectname]\V[x].[x].x
  - [ ] create folder V[x].[x].[x]
  - [ ] include a zipped folder: [projectname]-V[x].[x].[x]+[hash].zip
  - [ ] include the extracted [`LICENSE.txt`](LICENSE.txt)
  - [ ] include the extracted [`CHANGELOG.md`](CHANGELOG.md)
* [ ] communicate the download link
  - Any release (these are added to all and any of the other categories)
    - PO, PM, TM
  - General interest releases
    - Most demanding SDK clients/users
    - IoT Core (PO)
    - Moneo (PO)
    - OPC UA (PO)
  - Dedicated HotFix/Patch releases:
    - Patch requester
  - Internal/Development Releases
    - Interested development parties

