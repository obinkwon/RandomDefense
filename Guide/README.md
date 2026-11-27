# 랜덤직업디펜스 가이드

게임 아트를 만들고 Unity에 넣을 때 따르는 규칙 모음이다. 아래 Claude 아티팩트 내용과 현재 프로젝트 설정(`Assets/` 임포트 설정, 에디터 스크립트)을 바탕으로 정리했다. 기준일 2026-11-27.

| 문서 | 누가 보나 | 내용 |
| --- | --- | --- |
| [01_디자인_가이드.md](01_디자인_가이드.md) | 아트 제작자 | 화풍, 색, 픽셀 규격, 컴포넌트, 금지 규칙 |
| [02_퍼블리싱_가이드.md](02_퍼블리싱_가이드.md) | Unity 작업자 | 폴더, 파일 이름, 임포트 설정, 9-slice, 프리팹, HUD, 폰트 |
| [03_에셋_제작_절차.md](03_에셋_제작_절차.md) | 둘 다 | 생성부터 게임 반영까지 순서, 저장 위치 |
| [04_필요_아트_현황.md](04_필요_아트_현황.md) | 둘 다 | 아직 그림이 없는 캐릭터와 아이콘 |

## 원본 아티팩트

이 폴더의 문서는 아래 아티팩트를 옮겨 적은 것이다. 내용이 바뀌면 둘 다 고친다.

- [랜덤직업디펜스 게임 아트 가이드](https://claude.ai/artifact/CDzjQP4G558tzmo9NZ11KX): 01, 03의 원본
- [랜덤직업디펜스 공통 에셋 시안](https://claude.ai/artifact/4ygj7CdBxSzBh1sEBHUc6B): 맵 타일, 공통 UI, 이펙트, "게임 에셋으로 넣는 방법" 보드
- [랜덤직업디펜스 공통 이펙트 시안](https://claude.ai/artifact/AECsTxR9qBGtAPKMsXAQjR)
- [랜덤직업디펜스 유닛 스프라이트 시안](https://claude.ai/artifact/3n834QooZHAmmsEQVhNgF9)
- [랜덤직업디펜스 적유닛 스프라이트 시안](https://claude.ai/artifact/THxC7tKB28HyBCAGFuHrtN)
- [RandomDefense 필요 아트](https://claude.ai/artifact/BjDH88WM4psFhtp5toP6Kr): 04의 원본
- [랜덤 직업 디펜스 직업 밸런스 시트](https://claude.ai/artifact/QNzGBGJyJkFYGCwtXKBQxu): 캐릭터 외형·모션 입력

## 가이드와 프로젝트가 다른 곳

정리하면서 아티팩트 규칙과 실제 프로젝트가 맞지 않는 곳을 찾았다. 어느 쪽을 기준으로 할지 정해서 한쪽을 고쳐야 한다.

- [ ] **맵 타일 크기**: 아트 가이드는 64×64 격자, 실제 타일은 128×128 PNG(PPU 128)이다.
- [ ] **카운터 UI 조립 방식**: 아트 가이드는 "빈 패널 + 아이콘 + 숫자"를 게임에서 조립한다고 하지만, 실제 `counter_*.png`와 `btn_summon.png`는 아이콘이 들어간 한 장짜리 이미지(398×216)다.
- [ ] **UI 아이콘 32×32**: 아트 가이드에서 확정했지만, 아직 32×32 아이콘 파일은 `Assets/Art/Sprites/UI/`에 없다.
- [ ] **적 스프라이트 위치**: 슬라임은 `Sprites/Units/`에 있고 `Sprites/Enemy/`에는 `Enemy.png` 하나만 있다.
- [ ] **색 헥스값**: 아트 가이드는 "아직 없음"이지만, 등급색과 조합표 색은 코드에 이미 정해져 있다. 01 문서에 옮겨 적었다.
