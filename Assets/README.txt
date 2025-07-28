========================================
  🚀 Multi Shooting Game
========================================

[ 프로젝트 개요 ]
- 장르 : 2D 슈팅 게임 (Bullet Hell 스타일)
- 엔진 : Unity
- 네트워크 : Photon PUN 2
- 플랫폼 : PC

[ 제작 기간 & 참고 ]
- 제작 기간 : 약 2주
- 참고 강좌 : 골드메탈(Goldmetal) Unity 2D 슈팅게임 강좌

※ 본 프로젝트는 강좌를 기반으로 학습 및 포트폴리오 목적으로 제작되었습니다.
※ 모든 코드 구조는 직접 리팩토링 및 Photon 멀티플레이 지원으로 확장되었습니다.

[ 주요 변경점 ]
- 골드메탈 원본 강좌 구조 분석 후 전체 코드 리팩토링
- 싱글플레이 → Photon PUN 2 기반 멀티플레이로 전환
- PoolManager, NetworkManager, GameManager 등 시스템화
- UI 및 게임 로직을 멀티 환경에 맞게 수정

[ 게임 플레이 ]
1. 로비에서 방 생성 또는 참가 (2인 플레이 지원)
2. 다양한 적 스폰 패턴 및 보스 패턴
3. 코인, 파워업, 폭탄 아이템 드랍
4. 라이프/스코어 UI, 게임 클리어/오버 시스템

[ 프로젝트 구조 ]
- Scripts
   ├ Managers : GameManager, NetworkManager, PoolManager 등
   ├ Player : Player 컨트롤 및 Follower
   ├ Enemy : Enemy, Boss, AI 로직
   ├ UI : GameUIManager, LobbyUIManager
   ├ Common : CoroutineUtils, Enum, PoolData 등
   └ Effects : Explosion 등
- Prefabs
- Scenes
- Resources

[ 실행 방법 ]
1. 게임 실행 후 방 생성/참가
2. 한 명 또는 두 명 접속 시 게임 시작

[ 조작 방법 ]
■ PC 버전
   - 이동 : WASD 키 또는 방향키
   - 공격 : 마우스 좌클릭
   - 폭탄 : 마우스 우클릭
■ 모바일 버전
   - 이동 : 화면 왼쪽의 가상 조이스틱
   - 공격 : A 버튼
   - 폭탄 : B 버튼

========================================