# 📈 Beat The Market

A rhythm game where the falling notes ARE a live stock chart — hit your keys on beat to pump the line up, miss and watch it crash. Built solo in Unity.

## 🎮 Overview

Classic 4-lane rhythm gameplay (D/F/J/K) reskinned as a trading terminal. Every hit/miss plots a point on a scrolling chart in real time, so your performance literally draws the stock line. Fully data-driven — levels are JSON charts, not code.

## 🛠️ Systems I Built

- **Live Chart Graph** — Scrolling `LineRenderer` built from hit history, converting timing into X/Y positions every frame
- **JSON Chart Pipeline** — Loads chart + audio at runtime, pre-calculates note spawn timing/speed so notes land exactly on beat
- **Dynamic Difficulty** — Hitting notes speeds up the song via audio pitch scaling; missing slows it back down
- **Dual Fail States** — Lose by running out of money, or by leaving the chart inactive too long (crash timeout)
- **Tap & Hold Notes** — Hold notes resolve on release based on % held, with a shrinking hold-line visual
- **Full Pause/Resume** — Syncs `Time.timeScale` with explicit audio pause so nothing drifts

## 🧰 Tech Stack

`Unity` · `C#` · `TextMeshPro` · `LineRenderer` · `ScriptableObjects` · `UnityWebRequest`

## 👥 Team

Built in 1 month by a team of 3 as a class project.

## ▶️ How to Play

- **D / F / J / K** — Hit the note as it crosses the line
- Hold for hold notes, release too early = miss
- Hit your money target before the song ends to win
