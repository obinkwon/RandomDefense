# 랜덤직업디펜스 가이드

게임 아트를 만들고 Unity에 넣을 때 따르는 규칙 모음이다. 아래 Claude 아티팩트 내용과 현재 프로젝트 설정(`Assets/` 임포트 설정, 에디터 스크립트)을 바탕으로 정리했다. 기준일 2026-11-27.

| 문서 | 누가 보나 | 내용 |
| --- | --- | --- |
| [01_디자인_가이드.md](01_디자인_가이드.md) | 아트 제작자 | 화풍, 색, 픽셀 규격, 컴포넌트, 금지 규칙 |
| [02_퍼블리싱_가이드.md](02_퍼블리싱_가이드.md) | Unity 작업자 | 폴더, 파일 이름, 임포트 설정, 9-slice, 프리팹, HUD, 폰트 |
| [03_에셋_제작_절차.md](03_에셋_제작_절차.md) | 둘 다 | 생성부터 게임 반영까지 순서, 저장 위치 |
| [04_필요_아트_현황.md](04_필요_아트_현황.md) | 둘 다 | 아직 그림이 없는 캐릭터와 아이콘 |

디자인 캔버스 보드(시안·미리보기)는 [DesignPreview/](../DesignPreview/README.md)에, 기획서·요구사항·직업 조합·스토리 문서는 [Planning/](../Planning/README.md)에 옮겨 두었다. 움직이는 미리보기는 스크립트를 읽어야 해서 파일을 바로 열지 말고 로컬 서버로 연다: `python -m http.server 8765 --directory DesignPreview` → http://localhost:8765

## 원본 아티팩트

이 폴더의 문서는 아래 아티팩트를 옮겨 적은 것이다. 2026-11-27부터는 이 폴더와 [DesignPreview/](../DesignPreview/index.html)가 기준이고, 아티팩트는 처음 만든 원본으로만 남겨 둔다. 규칙을 바꿀 때는 이 폴더만 고치며, 아티팩트는 따로 정리하기로 할 때만 고친다.

- [랜덤직업디펜스 게임 아트 가이드](https://claude.ai/artifact/CDzjQP4G558tzmo9NZ11KX): 01, 03의 원본
- [랜덤직업디펜스 공통 에셋 시안](https://claude.ai/artifact/4ygj7CdBxSzBh1sEBHUc6B): 맵 타일, 공통 UI, 이펙트, "게임 에셋으로 넣는 방법" 보드
- [랜덤직업디펜스 공통 이펙트 시안](https://claude.ai/artifact/AECsTxR9qBGtAPKMsXAQjR): 옮기지 않음 (무시)
- [랜덤직업디펜스 유닛 스프라이트 시안](https://claude.ai/artifact/3n834QooZHAmmsEQVhNgF9): DesignPreview `unit-*.html`
- [랜덤직업디펜스 적유닛 스프라이트 시안](https://claude.ai/artifact/THxC7tKB28HyBCAGFuHrtN): DesignPreview `enemy-*.html`
- [RandomDefense 필요 아트](https://claude.ai/artifact/BjDH88WM4psFhtp5toP6Kr): 04의 원본
- [랜덤 직업 디펜스 직업 밸런스 시트](https://claude.ai/artifact/QNzGBGJyJkFYGCwtXKBQxu): 캐릭터 외형·모션 입력

## 가이드와 프로젝트 맞추기

2026-11-27에 정한 기준과 진행 상태.

- [x] **맵 타일 크기**: 프로젝트 기준 128×128(PPU 128)으로 가이드를 고침
- [x] **캐릭터 크기**: 맵 타일과 같은 밀도로 맞춤. 캐릭터 스프라이트를 절반으로 줄이고 PPU를 256에서 128로 바꿈 (게임 안 크기는 그대로)
- [x] **적 스프라이트 위치**: 슬라임을 `Sprites/Enemy/`로 옮김
- [x] **버튼 파일 이름**: `btn_normal` → `btn_normal_speed`, 프리팹 `Button_NormalSpeed`
- [x] **카운터 파일 이름**: `counter_life/stage/enemy` → `counter_barrier/wave/monster`, 프리팹도 같이 바꿈
- [x] **패널 크기**: 문서는 프로젝트 파일 크기로 통일 (01의 패널 목록). 디자인 캔버스 보드 쪽은 나중에 정리
- [x] **색 헥스값**: 코드에 있는 등급색과 글자색을 01에 옮겨 적음
- [x] **카운터·소환 버튼 조립 방식**: 빈 패널 `counter_panel` + 아이콘으로 바꿈. 패널과 아이콘은 공통 에셋 시안의 '공통 UI' 보드에서 가져옴. 아이콘이 들어간 한 장짜리 `counter_*.png`, `btn_summon.png`는 지움
- [x] **UI 아이콘 32×32**: `icon_crystal/barrier/wave/time/monster/summon.png` 6개를 32×32로 만듦
- [x] **`Sprites/Enemy/Enemy.png`**: 안 써서 지움
