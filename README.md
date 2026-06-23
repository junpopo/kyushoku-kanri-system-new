# 給食管理システム

C# Windows Forms で作る給食管理システムの最初の版です。

## 名簿読み込み

Excel の名簿は、Excel で「CSV UTF-8」に保存してから読み込みます。

対応している列名:

- `区分` または `type`: 職員 / 生徒 / ALT / 教育実習生 / 試食会 / ゲスト
- `学年` または `grade`
- `組`、`クラス`、`class`
- `番号`、`出席番号`、`number`
- `姓`、`苗字`、`lastName`
- `名`、`firstName`
- `氏名` または `name`: 姓と名の間に空白がある場合は分けて読み込みます
- `備考` または `memo`

転入、退職、年度途中の追加は、画面の「1人追加」から登録します。

## 保存場所

データは以下に保存します。

`Documents\KyushokuKanriSystem\data.json`
