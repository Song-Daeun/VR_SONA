# Amazing World of VR : 부루마불을 기반으로 하는 플래시 게임의 VR 재구성
본 프로젝트는 고전 게임의 VR 재구성이라는 주제로, 한국의 보드게임을 기반으로 하는 플래시게임인 "Amazing World of AmlaD"를 재구성하여 VR로 구현하는 것을 목표로 한다.

| 메인 게임 | 미션 1 - 농구 게임 | 미션 2 - 워터러시 게임 |
|:---:|:---:|:---:|
|<img src="https://github.com/user-attachments/assets/70ef7f77-2e8e-4b47-99a1-726e7a9b92f6" width="90%">|<img src="https://github.com/user-attachments/assets/496c267b-118d-42a5-a282-871a1efb964b" width="100%">|<img src="https://github.com/user-attachments/assets/3534bef4-aa06-4722-b9c1-d836d4902dd1" width="100%">|

기존 게임의 특색을 살리기 위해 게임을 진행하는 보드판 위를 플레이어가 움직이는 방식으로 구현하였으며, 게임을 진행할 때 시공간적인 제약을 제거하기 위해 1인 플레이로 구성하였다. 보드판 위에서 빙고를 완성해야 하는 새로운 게임 종료조건을 추가하여 사용자의 흥미와 동기를 유발하고자 하였다.
개발 게임 엔진은 Unity를 사용하였고, 가상현실 HMD는 Oculus Quest 2를 사용하였다.

[본 게임의 Build를 위해서는 프로젝트를 clone 받은 이후, 에셋과 SDK를 세팅하면 된다. 그에 대한 과정은 아래에 기재되어 있다.](#contents)

## Contents
- [Project Structure](#project-structure)
- [Environment Setting](#environment-setting)
- [User Manual](#user-manual)
- [MainGame Structure](#maingame-structure)
- [Mission 1 Structure](#mission-1-structure)
- [Mission 2 Structure](#mission-2-structure)
- [Role Distribution](#role-distribution)
- [Conclusion](#conclusion)
- [ETC](#etc)


## Project Structure
```
 🥽 VR_SONA
  ┣ 📂 Assets
  ┃ ┣ 📂 _TerrainAutoUpgrade
  ┃ ┣ 📂 Animations
  ┃ ┣ 📂 Arts
  ┃ ┣ 📂 Input
  ┃ ┣ 📂 Models
  ┃ ┣ 📂 Plugins
  ┃ ┣ 📂 Prefabs
  ┃ ┣ 📂 ProBuilder Data
  ┃ ┣ 📂 Resuorces
  ┃ ┣ 📂 Scenes
  ┃ ┃ ┣ ▶️ StartScene  // 게임 시작 화면
  ┃ ┃ ┣ 🎯 MainGameScene  // 메인 게임 진행
  ┃ ┃ ┣ 🏀 MissionBasketballScene  // 칸에 도달했을 시 진행하는 미션
  ┃ ┃ ┣ 🚰 MissionWaterRushScene  // 칸에 도달했을 시 진행하는 미션
  ┃ ┃ ┗ 🎲 DiceScene  // 주사위를 굴리고 결과값대로 플레이어 이동
  ┃ ┣ 📂 Scripts
  ┃ ┃ ┣ 📂 BasketBall  // 미션 1 로직 스크립트
  ┃ ┃ ┣ 📂 BGM  // 게임 배경음악 관리
  ┃ ┃ ┣ 📂 Core  // 게임 시스템 관리
  ┃ ┃ ┣ 📂 Player  // 플레이어 상태 확인
  ┃ ┃ ┣ 📂 UI  // 정적 UI 관리
  ┃ ┃ ┣ 📂 VR  // VR 인터랙션
  ┃ ┃ ┗ 📂 WaterRush  //미션 2 로직 스크립트
  ┃ ┣ 📂 Settings
  ┃ ┣ 📂 Systems
  ┃ ┣ 📂 TextMesh Pro
  ┃ ┣ 📂 Textures
  ┃ ┣ 📂 ThirdParty
  ┃ ┣ 📂 UI
  ┃ ┣ 📂 Unity.VisualScripting.Generated
  ┃ ┣ 📂 XR
  ┃ ┣ 📂 XRI
  ┣ 📄README.md
  ┗ 📄 .gitignore
```

## Environment Setting
* 구현 환경
  * CPU : Window 12th Gen Intel(R) Core(TM) i7-1255U   1.70 GHz / Apple M1 Pro
  * GPU : NVIDIA RTX 3060 Ti
  * Unity version : 2022.3.60f
  * OpenXR Plugin version : 1.14.3
  * XR Interaction Toolkit version : 2.6.4
  * XR Plugin Management version : 4.5.1
* 빌드 환경
  * HMD : Meta Quest 2
  * Cable : 링크 케이블을 통한 유선 연결 방식 / Wi-Fi 연결 시에는 데스크탑과 동일한 네트워크에 연결 후 Quest에서 에어링크 실행

## User Manual
본 프로젝트를 실행시키기 위해서는 다음과 같이 프로젝트를 clone 받은 후 Unity Hub에서 실행한다.
```
cd Desktop
git clone https://github.com/Song-Daeun/VR_SONA.git
```
이후 Unity Hub에서 Add project from disk를 선택한 후, clone한 폴더를 선택하여 프로젝트를 연다.
StartScene, MainGameScene, MissionBasketballScene, MissonWaterRushScene, DiceScene을 빌드한 후, 게임을 실행한다.

### MainGame Structure
<div align="center">
<img src="https://github.com/user-attachments/assets/08081b46-f428-45b1-b196-cf4f8291580c" width="70%">
</div>
StartScene의 게임 시작 버튼을 누르면 MainGameScene으로 전환된다. 게임의 모든 UI click interaction은 ray interactor를 사용해서 진행된다. 
제한 시간 8분 이내에 미션을 성공한 칸으로 2줄 이상의 빙고를 완성하면 게임은 종료된다. 게임 시작 시 플레이어에게 800 코인이 자동으로 지급되며, 미션을 한번씩 참여할 때마다 100코인씩 차감된다.


|DiceScene 로드|주사위 던지기|결과 출력|
|:---:|:---:|:---:|
|<img src="https://github.com/user-attachments/assets/4cd42f1d-7e56-482f-8f16-e9bb45b9e8be" width="78%">|<img src="https://github.com/user-attachments/assets/7f1308c7-3109-471b-a73e-2434e9cdecd1" width="95%">|<img src="https://github.com/user-attachments/assets/5f737331-ed87-4b01-9d49-5f4e688f47ba" width="100%">| 

이후 전환된 씬에 로드된 주사위 버튼을 클릭하면 DiceScene이 로드된다. 컨트롤러의 grab interaction을 사용하여 주사위를 던지면 결과값이 출력되고, 결과값에 해당하는 인덱스의 타일로 자동으로 이동한다. 미션 참여 여부를 묻는 창에서 사용자는 참여 여부를 결정할 수 있으며, 컨트롤러를 사용할 수 없을 경우에는 XR Device Simulator를 사용하여 게임을 플레이할 수 있다. Ray Interactor를 사용하여 UI를 클릭할 수 있으며, 예 버튼을 누를 경우 두 개의 미션 씬 중 하나의 씬이 로드되고, 아니오 버튼을 누를 경우 다시 주사위를 던질 수 있는 버튼이 로드된다. 

로드되는 미션 씬에서는 각각 다음과 같은 구조를 따른다.

### Mission 1 Structure
컨트롤러를 사용할 수 없는 환경을 고려하여 키보드와 컨트롤러를 함께 사용할 수 있도록 input system을 구현하였다. Mission 1인 농구 게임에서는 키보드 N키와 컨트롤러의 X버튼을 클릭하여 농구공을 던질 수 있다. 플레이어가 직접 농구공을 던지는 듯한 느낌을 제공하기 위해 1인칭 시점으로 구현하였으며, 제한시간 15초 내에 1골을 넣으면 미션을 성공한다.
|<img src="https://github.com/user-attachments/assets/964fad77-a2c9-46e9-b061-7aa927bc33a9" width="100%">|<img src="https://github.com/user-attachments/assets/e786a5de-6488-45a5-bac9-e2c0e4f64f56" width="100%">|
|:---:|:---:|


### Mission 2 Structure
Mission 2인 워터러시 게임에서는 키보드 스페이스키와 컨트롤러의 A버튼을 클릭하여 물줄기를 쏠 수 있다. 물이 앞으로 얼마나 나가는지를 확인하기 위해 3인칭 시점으로 구현하였으며, 제한시간 10초 내에 물줄기가 꽃에 도달하면 미션을 성공한다.
|<img src="https://github.com/user-attachments/assets/71cf96e0-2018-4542-bb4d-a0169bad4045" width="100%">|<img src="https://github.com/user-attachments/assets/3230ac57-d110-4018-b672-151f49c42e11" width="100%">|
|:---:|:---:|

메인 게임 내에서 마법서 칸에 도달하게 되면 두 가지의 이벤트를 경험할 수 있다.
<div align="center">

| 비행기 이벤트 | 시간 추가 |
|:---:|:---:|
|<img src="https://github.com/user-attachments/assets/368a9ed6-63f6-4b9d-8d82-3dc060825663" width="370">|<img src="https://github.com/user-attachments/assets/6ae0a272-c987-4255-a841-4d40db253989" width="400">|

</div>
마법서 칸에 도착하게 되면 책 구조물이 건설된 후 두 가지의 이벤트 중 하나가 랜덤하게 실행된다. 비행기 이벤트는 사용자가 원하는 칸으로 직접 이동할 수 있으며, 시간 추가 이벤트는 MainGame의 플레이 시간이 30초 추가된다. 

<div align="center">
<img src="https://github.com/user-attachments/assets/37018aa8-81b2-4afd-9252-5be55fc9468b" width="70%">
</div>
미션을 성공한 후 돌아가기 버튼을 클릭해 MainGameScene 돌아오면 건물이 건설된 것을 확인할 수 있다.

| 게임 성공 | 코인 부족 | 시간 초과 |
|:---:|:---:|:---:|
|<img src="https://github.com/user-attachments/assets/14411d1a-ade2-4f94-a546-a14f566aee5e" width="80%">|<img src="https://github.com/user-attachments/assets/9fa68059-330e-423e-9232-c62ffc9a2783" width="80%">|<img src="https://github.com/user-attachments/assets/a6fb639d-f4a9-4f0d-9cca-daed0f00eb6d" width="80%">|

빙고를 완성하는 데에 성공하게 되면 성공을 알려주는 패널이 표시된다. 코인을 다 소모하여 미션에 더 이상 참가할 수 없거나, 제한 시간 8분을 넘겼을 경우 게임에 실패했다는 패널이 표시된다. 다시 시작 버튼을 클릭하면 게임을 재시작할 수 있으며, 나가기를 누르면 게임이 종료된다.

## Role Distribution
| **송다은(팀장)** | **이송은(팀원)** | **이나영(팀원)** |
|:---:|:---:|:---:|
| • 씬 에셋 배치<br>• 미션 2 개발<br>• BGM 및 애니메이션 관리 | • 미션 및 메인 게임 연동 시스템 관리<br>• 미션 1 개발 | • VR 연동 및 가상현실 환경 구축<br>• 메인게임 통합<br>• 플레이어 상태 관리 |

## Conclusion
기존 게임의 특징을 유지하며 VR로 재구성하여 사용자에게 몰입감 있는 동적 경험을 제공하여 기존 플래시 게임의 한계를 극복하고자 하였다. 사용자가 가상현실 내의 공간을 탐색하고 직접 주사위를 굴리는 물리적인 동작을 통해 몰입감을 더욱 향상시키고자 하였으며, 제한 시간 내에 목표 달성이라는 긴장감 있는 플레이를 제공하고자 하였다.

## ETC
* 본 프로젝트는 한양대학교 에리카 캠퍼스 인공지능학과 가상및증강현실프로그램의 IC-PBL 프로젝트이다.
  
* Team SONA
  * 송다은 : sde11280@hanyang.ac.kr
  * 이나영 : lwg2326@hanyang.ac.kr 
  * 이송은 : lc26482648@gmail.com
