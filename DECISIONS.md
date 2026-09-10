# DECISIONS.md

Bright Path Learning Centre — scheduling tool.
**Pinned date:** the tool treats "today" as **2026-03-05 (Thursday)**, inside the seed week (3–10 Mar 2026).

---

## Summary

**The problem.** Bright Path runs 12 tutors and ~200 families out of one shared spreadsheet that only Mai fully understands, and she leaves in 8 weeks. Nothing stops a bad booking being typed in. Of 34 lessons in the seed week, **13 (38.2%) are touched by a clash or an overloaded tutor day**: 2 tutor-clash incidents (4 lessons, 11.8%; one pair also double-books a room), 1 student-clash incident (2 lessons, 5.9%) — the exact failure the owner opened with — and 1 tutor day over the 6-booking cap (7 lessons, 20.6%).

**The solution.** Clash detection enforced at write time: a lesson slot is unique on **student, tutor and room**, and any overlapping booking is refused rather than accepted and cleaned up later. One interval-overlap rule, three columns, one endpoint.

**Why this one.** It is the item the owner made non-negotiable, the only failure that reaches the customer, and the only one that stops depending on Mai's memory. It also resolves the tutor, student and room clashes above — 6 lessons (4 tutor + 2 student; the room clash is the same 2 tutor-clash rows) out of the 13, 46.2% of the flagged set — for one rule. The 6-booking cap accounts for the other 7 lessons, is the single largest category by row count, and is deliberately left for next: it reuses the same per-tutor-per-day query and is roughly an hour's work.

---

## 1. Questions for the owner

**Q1 — Does an exam pair count as one booking or two?**
Mai deliberately puts two students with one tutor in one room in one slot (L009+L010), but the rules say a tutor is in one room at a time. Both cannot be true.
- *One* → the slot holds a set of students; the clash rule needs an exception.
- *Two* → the pair **is** a double-booking, and enforcing the rule bans something the families like.
- **Without an answer:** treated as two clashing rows. The owner's "never again" outranks the receptionist's habit — but this is the answer I most want, because it is the only place enforcement removes something the centre currently sells.

**Q2 — Refuse, or accept-and-flag?**
- *Refuse* → hard 409, no override; the rule actually holds.
- *Flag* → the booking lands with a warning. Kinder at 8am, but that is the spreadsheet with extra steps.
- **Did:** refuse. "If the system allows it, the system is broken" is the closest thing here to a spec.

**Q3 — What does the 16:00 cut-off forbid?**
- *Blocks new bookings* → timestamp check on create.
- *Only changes how edits are recorded* → nothing blocked; every late edit writes an audit row and shows as **changed**.
- **Did:** the second. The tutor's complaint is not knowing which message is real, not lateness. Blocking edits pushes them back to WhatsApp — the thing we are replacing.

**Q4 — Is "mid-morning to mid-evening" a rule or a description?**
No hours are given; the week runs 09:00–21:30. **Did:** treated as description. I will not invent a boundary precise enough to reject bookings with.

**Q5 — Six rooms, but only R1–R3 ever appear. Unusable, or just habit?**
Changes whether a clash is usually fixable by moving rooms. Affects the error message I can offer, not the rule. **Did:** trusted the brief's six.

---

## 2. Where the brief does not hold together

- **Rules vs. data, by design.** "What the centre actually ran that week is not always what the rules say should have happened." Every violation below is a fact about the week, not a broken export. Rules = target state; data = evidence of what breaks without enforcement.
- **The exam pair is forbidden by the same brief that describes it.** See Q1.
- **The owner's headline complaint is not in the rules table.** He leads with a *student* in two places at once; the table constrains only tutors and rooms. The data contains exactly that case (L007+L008). I treated the student as a third uniqueness axis on the strength of the quote.
- **"No more than 6 bookings" never defines a booking.** Row, or slot? With an exam pair those differ.

---

## 3. Assumptions I invented

1. **A slot is unique on student, tutor and room.** Two lessons clash if they overlap and share any one.
2. **Overlap is half-open:** `startA < endB && startB < endA`. Back-to-back is fine; no gap required.
3. **`cancelled` frees everything; `no_show` frees nothing** — and the daily cap counts `booked` + `no_show` only.
4. **An exam pair counts as two bookings** (consequence of Q1).
5. **Lessons are atomic** — a clash rejects the whole booking; the cap is per calendar day.
6. **A move is validated exactly like a create,** keeping its lesson id and writing an audit row. L032 kept its id when it moved to a Monday, which is how the closed-day lesson got in unnoticed.
7. **All times are Vietnam local (+07:00), single site.**
8. **Pre-existing violations still load.** The importer records them rather than refusing. A tool that cannot open Monday's real data is useless on Monday.

---

## 4. What the week's data shows

34 rows: 31 booked, 2 cancelled, 1 no-show.

| # | Violation | Incidents | Lessons | % of 34 | Evidence |
|---|---|---|---|---|---|
| 1 | **Tutor double-booked** | 2 | 4 | 11.8% | L009+L010 (T1, Wed 11:00, both R1, "exam pair"); L033+L034 (T1, Tue 09:00, R1 **and** R2) |
| 2 | **Tutor over 6/day** | 1 | 7 | 20.6% | T1 on Fri 6 Mar — seven lessons, 09:00 to a 21:30 finish |
| 3 | **Student double-booked** | 1 | 2 | 5.9% | L007+L008 (Le Minh Chau, Wed 09:00, T3/R3 **and** T2/R2, "added by phone") |
| 4 | **Room double-booked** | 1 | 2 | 5.9% | L009+L010 in R1 — same rows as #1 |
| 5 | **Lesson on a closed day** | 1 | 1 | 2.9% | L032 on Mon 9 Mar, "moved from Sunday" |
| 6 | **Late cancellation (≤4h)** | 1 | 1 | 2.9% | L017, 1.3h before start — chargeable, tutor still paid |
| 7 | **No-show** | 1 | 1 | 2.9% | L015 — frees neither room nor slot |

Not violations: L005 was cancelled 5.8h ahead, **outside** the 4-hour window and therefore free — the week's only clean cancellation. L027 finishes 21:30, which stretches "mid-evening" but breaks no stated rule.

**Eleven of thirty-four rows — one in three — are part of a double-booking or an overloaded day.** Every one is a write that should have been refused as Mai typed it. Rows 5–7 are each a single row, already visible in the sheet, and none of them sends a family to a lesson nobody is teaching.

---

## 5. Features, and the one I built

- A day view: open the laptop, see today, do not scroll.
- **Clash detection on write — a slot is unique on student, tutor and room.**
- Daily tutor cap of six, enforced at booking time.
- Cancel / no-show as distinct statuses, with the 4-hour line deciding chargeability.
- A change log, so a post-16:00 edit shows as a change instead of a silent overwrite.
- Generated tutor day sheets, instead of WhatsApp.

### Built: clash detection

- **The owner made it non-negotiable** — *"That can never happen again — if the system allows it, the system is broken."*
- **It is the only failure that reaches the customer.** A family drives in and the centre finds out when they are at the desk. Everything else fails quietly and Mai patches it next morning.
- **One rule, one code path.** Overlap + shared student/tutor/room is a single predicate over three columns — it catches rows 1, 3 and 4 for the price of one.
- **It is the item that stops depending on Mai.** The cap, the cancellation window and the change log all still half-work in the spreadsheet because she holds them in her head. The clash rule is the one she has been observed breaking twice in seven days, and she leaves in eight weeks.

**Why the tutor clash is the anchor:** two of the three clash incidents are tutor clashes, and it is the one with a live disagreement behind it — Mai's exam pair. Getting it right forces the Q1 conversation, and Q1 decides the schema. Student and room are the same predicate over different columns.

**What I leave broken:**

- **The daily cap is not enforced.** T1's seven-lesson Friday still stands — the largest violation in the week by row count. Next thing I would build; roughly an hour, because it reuses the per-tutor-per-day query the clash check already runs.
- **The closed-day move stands.** L032 stays on a Monday.
- **Nothing renders.** No day view, so Mai still cannot open the laptop and see today — the thing the owner actually asked for.
- **No change log,** so the tutor's "which message is real" problem is untouched.
- **The exam pair is banned on one reading of an ambiguous rule.** If Q1 goes the other way, I have removed a product the families like, and the fix is a schema change.

---

## 6. The rules, as enforced

| Rule | Status | Where |
|---|---|---|
| Slot unique on student / tutor / room | Enforced — reject overlapping writes | Code |
| Tutor ≤ 6 bookings/day | **Not built** — next up | Code |
| Cancel ≤4h before start → chargeable | Flag on the row | Code |
| Cancelled frees slot; no-show does not | Status drives the clash query | DB enum + code |
| Six rooms, one lesson each | Enforced | Code |
| Tomorrow final at 16:00 | **Not built** | — |
| Closed Mondays | **Not built** | — |

Overlap and counts live in code, not database constraints. An exclusion constraint would catch tutor and room clashes, but the student column and the cancelled/no-show asymmetry both need conditional logic — and splitting one rule across two enforcement points is how the halves drift apart.

---

## 7. Reflection

**Next week:** the cap (biggest violation, cheapest add), then the day view (he asked for it in plain words and I built him an API he cannot see), then reschedule validation.

**Known weak:** the exam-pair call is a coin-flip on an ambiguous rule and I have committed a schema to it. No index strategy — at 200 families the clash query scans a day's rows, which is fine and will stay fine, but unproven. The seed importer loads violations, so the database starts in a state the rules forbid.

**Where AI helped:** parsing the week into clash pairs and per-tutor day counts — the §4 table came out of a throwaway script, and it is what moved me off my first instinct. Also the interval-overlap boundary tests, where I would have hand-written three of six cases and missed touching endpoints.

**A suggestion I threw away:** an `is_exam_pair` flag letting a clash through when both rows carried it. Tidy, and it would have kept Mai working. I dropped it because it encodes an unanswered question as a permanent feature — the flag ships, gets used, and quietly makes "never again" untrue. A double-booking with paperwork is still a double-booking. Better to enforce, be wrong visibly in week one, and change it when he answers Q1.