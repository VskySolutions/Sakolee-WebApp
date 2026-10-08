<template>
  <q-page padding class="parent-dashboard">
    <!-- ---- Greeting ---- -->
    <div>
      <h1 class="parent-dashboard__title">{{ greeting }}{{ firstName ? `, ${firstName}` : "" }}!</h1>
      <p class="parent-dashboard__subtitle">Here's what's happening at the academy today.</p>
    </div>

    <q-banner v-if="error" dense rounded class="bg-red-1 text-negative">
      <template #avatar><q-icon name="o_error" color="negative" /></template>
      {{ error }}
      <template #action>
        <q-btn flat dense no-caps color="negative" label="Retry" @click="load" />
      </template>
    </q-banner>

    <!-- ---- KPI cards ---- -->
    <div class="parent-dashboard__stats">
      <div class="stat-card">
        <div class="stat-card__label">
          Next Class
          <span v-if="nextClass" class="student-chip">{{ nextClass.studentName }}</span>
        </div>
        <q-skeleton v-if="loading" type="text" width="60%" class="stat-card__value" />
        <template v-else>
          <div class="stat-card__value">{{ nextClass ? nextClass.className : "—" }}</div>
          <div class="stat-card__sub">{{ nextClass ? nextClass.when : "No upcoming classes this week" }}</div>
        </template>
      </div>
      <!-- Balance and messages have no data source yet; they render as "—" rather than a misleading 0. -->
      <div class="stat-card">
        <div class="stat-card__label">Balance</div>
        <div class="stat-card__value">—</div>
        <div class="stat-card__sub">Billing isn't available yet</div>
      </div>
      <div class="stat-card">
        <div class="stat-card__label">Messages</div>
        <div class="stat-card__value">—</div>
        <div class="stat-card__sub">Messaging isn't available yet</div>
      </div>
    </div>

    <!-- ---- Quick actions ---- -->
    <div>
      <h2 class="section-title">Quick Actions</h2>
      <div class="quick-actions">
        <button v-for="action in quickActions" :key="action.key" type="button" class="qa-btn" @click="action.run">
          <q-icon :name="action.icon" size="26px" color="primary" />
          {{ action.label }}
        </button>
      </div>
    </div>

    <!-- ---- Today's schedule ---- -->
    <div>
      <div class="section-head">
        <h2 class="section-title">Today's Schedule</h2>
        <button type="button" class="link-action" @click="notify.info('The monthly calendar isn\'t available yet.')">
          View Monthly Calendar
        </button>
      </div>

      <div class="schedule-list">
        <template v-if="loading">
          <q-skeleton v-for="n in 2" :key="n" type="rect" height="104px" class="schedule-skeleton" />
        </template>
        <div v-else-if="!todaySchedule.length" class="empty-card">
          <q-icon name="o_event_available" size="28px" color="primary" />
          No classes scheduled for today.
        </div>
        <div
          v-for="(entry, index) in todaySchedule"
          v-else
          :key="entry.key"
          class="schedule-card"
          :class="{ 'schedule-card--alt': index % 2 === 1 }"
        >
          <div class="schedule-card__time">
            <div class="t1">{{ entry.time }}</div>
            <div class="t2">{{ entry.meridiem }}</div>
            <div v-if="entry.minutes" class="t3">{{ entry.minutes }} min</div>
          </div>
          <div class="schedule-card__info">
            <h4>{{ entry.className }} - <span class="student-chip">{{ entry.studentName }}</span></h4>
            <div class="schedule-card__meta">
              <span v-if="entry.room"><q-icon name="o_meeting_room" size="14px" />{{ entry.room }}</span>
              <span v-if="entry.instructor"><q-icon name="o_person" size="14px" />{{ entry.instructor }}</span>
            </div>
          </div>
          <q-btn
            unelevated no-caps
            color="primary"
            label="Check-in"
            class="schedule-card__btn"
            @click="notify.info('Check-in isn\'t available yet.')"
          />
        </div>
      </div>
    </div>

    <!-- ---- Announcements ---- -->
    <div>
      <h2 class="section-title">Announcements</h2>
      <!-- Announcements have no endpoint yet. -->
      <div class="empty-card">
        <q-icon name="o_campaign" size="28px" color="primary" />
        No announcements right now.
      </div>
    </div>
  </q-page>
</template>

<script setup>
import { computed, onMounted } from "vue";
import { useRouter } from "vue-router";
import { useAuthStore } from "stores/auth";
import { Permissions, usePermissions } from "composables/usePermissions";
import { useNotify } from "composables/useNotify";
import { relativeDay, runsOn, shortTime, studentFullName, toMinutes, nextSessionOf, useParentPortalData } from "composables/useParentPortal";

const router = useRouter();
const auth = useAuthStore();
const notify = useNotify();
const { has } = usePermissions();
const { loading, error, students, classesOf, roomOf, instructorOf, load } = useParentPortalData();

const firstName = computed(() => (auth.user?.displayName || "").trim().split(/\s+/)[0] || "");

const greeting = computed(() => {
  const hour = new Date().getHours();
  if (hour < 12) return "Good morning";
  if (hour < 17) return "Good afternoon";
  return "Good evening";
});

const quickActions = computed(() => [
  {
    key: "schedule",
    label: "Schedule",
    icon: "o_calendar_month",
    run: () => (has(Permissions.ClassesRead) ? router.push("/classes") : notify.info("The schedule isn't available yet."))
  },
  { key: "activities", label: "Activities", icon: "o_campaign", run: () => notify.info("Activities aren't available yet.") },
  { key: "payments", label: "Payments", icon: "o_credit_card", run: () => notify.info("Payments aren't available yet.") },
  { key: "messages", label: "Messages", icon: "o_chat_bubble_outline", run: () => notify.info("Messages aren't available yet.") }
]);

// One entry per (student, class) pair — two siblings in the same class are two check-ins.
const enrollments = computed(() =>
  students.value.flatMap((s) => classesOf(s).map((cls) => ({ cls, studentId: s.studentId, studentName: studentFullName(s) }))));

const toEntry = ({ cls, studentId, studentName }) => {
  const start = toMinutes(cls.startTime);
  const end = toMinutes(cls.endTime);
  let minutes = start !== null && end !== null ? end - start : null;
  if (minutes !== null && minutes < 0) minutes += 24 * 60;
  const [time = "—", meridiem = ""] = shortTime(cls.startTime).split(/\s+/);
  return {
    key: `${studentId}-${cls.classId}`,
    className: cls.className || "Class",
    studentName,
    start,
    startLabel: shortTime(cls.startTime),
    time,
    meridiem,
    minutes,
    room: roomOf(cls),
    instructor: instructorOf(cls)
  };
};

const byStart = (a, b) => (a.start ?? 9999) - (b.start ?? 9999);

const todaySchedule = computed(() => {
  const today = new Date();
  return enrollments.value.filter(({ cls }) => runsOn(cls, today)).map(toEntry).sort(byStart);
});

const nextClass = computed(() => {
  const upcoming = enrollments.value
    .map((e) => ({ e, session: nextSessionOf(e.cls) }))
    .filter((x) => x.session && x.session.offset < 7)
    .sort((a, b) => a.session.offset - b.session.offset || byStart(toEntry(a.e), toEntry(b.e)));
  if (!upcoming.length) return null;
  const { e, session } = upcoming[0];
  const entry = toEntry(e);
  const dayLabel = relativeDay(session);
  return { ...entry, when: entry.startLabel ? `${dayLabel} at ${entry.startLabel}` : dayLabel };
});

onMounted(load);
</script>

<style scoped>
.parent-dashboard {
  display: flex;
  flex-direction: column;
  gap: 32px;
}

.parent-dashboard__title {
  font-size: 40px;
  font-weight: 700;
  letter-spacing: -0.8px;
  line-height: 1.2;
  margin: 0;
  color: var(--primary);
}

.parent-dashboard__subtitle {
  font-size: 16px;
  color: var(--on-surface-variant);
  margin: 6px 0 0;
}

/* ---- KPI cards ---- */
.parent-dashboard__stats {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 24px;
}

.stat-card {
  background: var(--surface-container-lowest);
  border: 1px solid rgba(199, 196, 215, 0.3);
  border-radius: 12px;
  padding: 24px;
  box-shadow: 0 4px 10px rgba(15, 23, 42, 0.05);
}

.stat-card__label {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 8px;
  font-size: 12px;
  font-weight: 500;
  letter-spacing: 0.6px;
  text-transform: uppercase;
  color: var(--on-surface-variant);
}

.stat-card__value {
  font-size: 28px;
  font-weight: 600;
  margin-top: 8px;
  color: var(--primary);
}

.stat-card__sub {
  font-size: 14px;
  margin-top: 6px;
  color: var(--on-surface-variant);
}

.student-chip {
  background: var(--primary);
  color: #fff;
  font-size: 15px;
  font-weight: 400;
  border-radius: 8px;
  padding: 4px 8px;
  text-transform: capitalize;
  letter-spacing: normal;
  white-space: nowrap;
}

/* ---- Sections ---- */
.section-title {
  font-size: 22px;
  font-weight: 600;
  line-height: 1.3;
  color: var(--on-surface);
  margin: 0 0 20px;
}

.section-head {
  display: flex;
  align-items: center;
  justify-content: space-between;
  flex-wrap: wrap;
  gap: 12px;
}

.link-action {
  background: none;
  border: none;
  padding: 0;
  margin-bottom: 20px;
  color: var(--primary);
  font-weight: 600;
  font-size: 14px;
  letter-spacing: 0.7px;
  cursor: pointer;
}

/* ---- Quick actions ---- */
.quick-actions {
  display: flex;
  flex-wrap: wrap;
  gap: 16px;
}

.qa-btn {
  flex: 1;
  min-width: 140px;
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 14px;
  padding: 25px 20px;
  background: rgba(96, 99, 238, 0.1);
  border: 1px solid rgba(96, 99, 238, 0.2);
  border-radius: 12px;
  color: var(--on-surface);
  font-weight: 600;
  font-size: 14px;
  letter-spacing: 0.7px;
  cursor: pointer;
}

.qa-btn:hover { background: rgba(96, 99, 238, 0.18); }

/* ---- Schedule ---- */
.schedule-list {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.schedule-skeleton { border-radius: 16px; }

.schedule-card {
  --accent: var(--primary);
  position: relative;
  overflow: hidden;
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: 24px;
  padding: 25px;
  background: var(--surface-container-lowest);
  border: 1px solid rgba(199, 196, 215, 0.4);
  border-radius: 16px;
}

.schedule-card--alt { --accent: #4e45d5; }

.schedule-card::before {
  content: "";
  position: absolute;
  left: 2px;
  top: 16px;
  bottom: -16px;
  width: 6px;
  border-radius: 99px;
  background: var(--accent);
}

.schedule-card__time { width: 100px; flex-shrink: 0; }
.schedule-card__time .t1 { font-size: 26px; font-weight: 700; color: var(--accent); }
.schedule-card__time .t2 {
  font-size: 12px;
  font-weight: 700;
  letter-spacing: 0.35px;
  text-transform: uppercase;
  color: var(--accent);
  opacity: 0.7;
}
.schedule-card__time .t3 { font-size: 13px; font-style: italic; color: var(--outline); margin-top: 4px; }

.schedule-card__info { flex: 1; min-width: 180px; }
.schedule-card__info h4 {
  margin: 0 0 10px;
  font-size: 22px;
  font-weight: 600;
  line-height: 1.4;
  color: var(--on-surface);
}

.schedule-card__meta {
  display: flex;
  flex-wrap: wrap;
  gap: 32px;
  font-size: 14px;
  color: var(--on-surface-variant);
}

.schedule-card__meta span { display: flex; align-items: center; gap: 8px; }

.schedule-card__btn {
  background: var(--accent) !important;
  border-radius: 12px;
  padding: 8px 20px;
  font-weight: 600;
  letter-spacing: 0.7px;
}

/* ---- Empty states ---- */
.empty-card {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 24px;
  background: var(--surface-container-lowest);
  border: 1px dashed var(--outline-variant);
  border-radius: 16px;
  color: var(--on-surface-variant);
  font-size: 14px;
}

@media (max-width: 1100px) {
  .parent-dashboard__stats { grid-template-columns: 1fr; }
}

@media (max-width: 600px) {
  .parent-dashboard__title { font-size: 30px; }
}
</style>
