# 🚀 Multi Shooting Game

<img width="1300" height="1080" alt="multishootinggame" src="https://github.com/user-attachments/assets/235d3c39-2df0-41a6-8e2a-590521901b4a" />

이 프로젝트는 **골드메탈(Goldmetal)** 님의 Unity 2D 슈팅게임 강좌를 기반으로 제작되었으며,

기존 싱글플레이 게임 구조를 **C# 리팩토링 + Photon PUN 2 멀티플레이 지원**으로 확장한 포트폴리오용 프로젝트입니다.

---

## 🎮 프로젝트 개요

- **장르:** 2D 슈팅 게임 (Bullet Hell 스타일)
- **엔진:** Unity
- **네트워크:** Photon PUN 2
- **플랫폼:** PC

---


## 📌 주요 변경점 및 작업 내용

✅ **골드메탈 원본 강좌 구조 분석 후 전체 코드 리팩토링**

✅ **싱글플레이 → Photon PUN 2 기반 멀티플레이로 전환**

✅ **PoolManager, NetworkManager, GameManager 등 시스템화**

✅ **UI 및 게임 로직을 멀티 환경에 맞게 수정**

---

## 📂 프로젝트 구조

Assets/

├── Scripts/

│   ├── Managers/        # GameManager, NetworkManager, PoolManager 등

│   ├── Player/          # Player 컨트롤 및 Follower

│   ├── Enemy/           # Enemy, Boss, AI 로직

│   ├── UI/              # GameUIManager, LobbyUIManager

│   ├── Common/          # CoroutineUtils, Enum, PoolData 등

│   └── Effects/         # Explosion 등

├── Prefabs/

├── Scenes/

├── Resources/

└── README.md

---

## 🕹️ 게임 플레이

- Photon 방 생성/참가 후 싱글 or 2인 플레이 가능
- 다양한 적 스폰 패턴과 보스 패턴
- 코인, 파워업, 폭탄 아이템 드랍
- 라이프/스코어 UI 및 게임 클리어/게임 오버 시스템

---

## 📌 제작 기간 & 크레딧

- **제작 기간:** 약 2주
- **참고 강좌:** [골드메탈 Unity 2D 슈팅게임 강좌](https://www.youtube.com/@Goldmetal)

> ⚠️ 본 프로젝트는 강좌를 기반으로 학습 및 포트폴리오 용도로 제작되었습니다.
> 
> 
> 코드 구조는 직접 재작성 및 네트워크화, 시스템화를 거쳤습니다.
> 

---

## 🚀 실행

1. 게임 실행 후 방 생성/참가
2. 한 명 또는 두 명 접속 시 게임 시작

---

## 🎮 조작 방법

### 💻 PC 버전

- **이동** : `WASD` 키 또는 `방향키`
- **공격** : `마우스 좌클릭`
- **폭탄** : `마우스 우클릭`

### 📱 모바일 버전

- **이동** : 화면 왼쪽의 **가상 조이스틱**
- **공격** : **A 버튼**
- **폭탄** : **B 버튼**
