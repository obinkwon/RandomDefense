# DesignPreview

디자인 캔버스 3개의 보드를 HTML/CSS로 옮긴 폴더다. 지금은 이 폴더가 기준이고, 캔버스는 처음 만든 원본으로만 남겨 둔다.

- [랜덤직업디펜스 공통 에셋 시안](https://claude.ai/artifact/4ygj7CdBxSzBh1sEBHUc6B): 맵 타일, 공통 UI, 이펙트, 조합 미리보기, 참고 템플릿
- [랜덤직업디펜스 유닛 스프라이트 시안](https://claude.ai/artifact/3n834QooZHAmmsEQVhNgF9): `unit-*.html`
- [랜덤직업디펜스 적유닛 스프라이트 시안](https://claude.ai/artifact/THxC7tKB28HyBCAGFuHrtN): `enemy-*.html`

## 실행 방법

움직이는 미리보기는 `dc-lite.js`를 불러와야 해서 로컬 서버로 연다. Python이 필요하다.

1. PowerShell에서 서버를 띄운다.

   ```powershell
   python -m http.server 8765 --directory D:\source\git\RandomDefense\DesignPreview
   ```

   프로젝트 폴더(`RandomDefense`)에서 실행한다면 경로를 줄여 써도 된다.

   ```powershell
   python -m http.server 8765 --directory DesignPreview
   ```

2. 브라우저에서 http://localhost:8765 를 연다.
3. 목록에서 보고 싶은 보드를 누른다.
4. 다 보면 서버를 띄운 창에서 `Ctrl+C`로 끈다.

`index.html`을 더블클릭해서 바로 열면 정적 시안은 보이지만, 움직이는 미리보기 3개는 빈 화면이 된다. 이때는 위 방법으로 연다.

Claude Code 데스크톱 앱에서는 `.claude/launch.json`의 `design-preview` 설정으로 같은 서버를 띄울 수 있다.

## 들어 있는 것

| 파일 | 내용 | 종류 |
| --- | --- | --- |
| `index.html` | 보드 목록 | |
| `map-tiles.html` | 맵 타일 | 정적 |
| `common-ui.html` | 공통 UI | 정적 |
| `effects.html` | 공통 이펙트 시안 | 정적 |
| `effect-frames.html` | 이펙트 프레임 (게임용으로 자름) | 정적 |
| `combine-preview.html` | 화면 배치 미리보기 (선택·사거리·조합 모드·소환) | 움직임 |
| `combo-popup.html` | 조합표 팝업 미리보기 | 움직임 |
| `combo-table.html` | 조합표 팝업 (단독) | 움직임 |
| `asset-guide.html` | 게임 에셋으로 넣는 방법 | 정적 |
| `asset-template.html` | 참고 템플릿 | 정적 |
| `effect-template.html` | 이펙트 참고 템플릿 | 정적 |
| `unit-swordsman.html` | 검사 · 대기와 공격 모션 (좌·우) | 정적 |
| `unit-archer.html` | 궁수 · 대기와 공격 모션 (좌·우) | 정적 |
| `unit-base-body.html` | 기본 몸 템플릿 (좌·우) | 정적 |
| `unit-concept.html` | 요리사 유닛 스프라이트 시안 1·2 (초기 화풍 비교) | 정적 |
| `unit-template.html` | 직업 유닛 참고 템플릿 | 정적 |
| `enemy-slime.html` | 슬라임 · 기본과 이동 모션 (좌·우) | 정적 |
| `enemy-slime-king.html` | 슬라임 킹 (보스) · 기본과 이동 모션 (좌·우) | 정적 |
| `enemy-template.html` | 적 유닛 참고 템플릿 | 정적 |
| `dc-lite.js` | 움직이는 미리보기용 작은 런타임 | |
| `assets/` | 보드에 쓰인 이미지 195장 (파일 이름은 캔버스의 에셋 id) | |

## 원본이 바뀌었을 때

이 폴더는 2026-11-27 시점의 캔버스를 옮긴 것이다. 이후로는 이 폴더를 직접 고친다. 캔버스를 다시 고쳤다면 자동으로 따라오지 않으니, 다시 옮겨 달라고 Claude에게 요청한다.
