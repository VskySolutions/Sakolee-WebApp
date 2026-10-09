<template>
  <q-page padding class="parent-students">
    <!-- ---- Header ---- -->
    <div class="parent-students__header">
      <div>
        <q-breadcrumbs class="text-brown">
          <template #separator>
            <q-icon size="1.0em" name="o_chevron_right" color="primary" />
          </template>
          <q-breadcrumbs-el label="Home" to="/" class="text-86 fw-700 text-uppercase sfs-11" />
          <q-breadcrumbs-el label="My Students" class="text-primary fw-700 text-uppercase sfs-11" />
        </q-breadcrumbs>
        <h1 class="parent-students__title">My Students</h1>
        <p class="parent-students__subtitle">View and manage your student's information, enrollments, and academic progress.</p>
      </div>
      <q-btn
        unelevated no-caps
        color="primary"
        icon="o_add"
        label="Add Student"
        class="parent-students__add"
        @click="openAdd"
      />
    </div>

    <q-banner v-if="error" dense rounded class="bg-red-1 text-negative">
      <template #avatar><q-icon name="o_error" color="negative" /></template>
      {{ error }}
      <template #action>
        <q-btn flat dense no-caps color="negative" label="Retry" @click="load" />
      </template>
    </q-banner>

    <template v-if="loading">
      <q-skeleton v-for="n in 2" :key="n" type="rect" height="320px" class="student-card" />
    </template>

    <div v-else-if="!cards.length" class="empty-card">
      <q-icon name="o_person_off" size="28px" color="primary" />
      No students are linked to your family yet.
    </div>

    <!-- ---- One card per student ---- -->
    <div v-for="card in cards" v-else :key="card.studentId" class="student-card">
      <div class="student-card__head">
        <div class="student-card__photo">{{ card.initials }}</div>
        <div class="student-card__identity">
          <div class="student-card__name-row">
            <h4>{{ card.name }}</h4>
            <span class="badge" :class="card.active ? 'badge--success' : 'badge--neutral'">
              {{ card.active ? "Active" : "Inactive" }}
            </span>
          </div>
          <div class="student-card__meta">
            <span v-if="card.age"><q-icon name="o_person" size="15px" />{{ card.age }}</span>
            <span v-if="card.gradeLevel"><q-icon name="o_school" size="15px" />{{ card.gradeLevel }}</span>
          </div>
        </div>
        <button type="button" class="icon-round" @click="openView(card.studentId)">
          <q-icon name="o_visibility" size="22px" />
          <q-tooltip>View details</q-tooltip>
        </button>
      </div>

      <div class="student-card__stats">
        <div class="mini-stat">
          <div class="mini-stat__icon"><q-icon name="o_description" size="22px" /></div>
          <div>
            <div class="mini-stat__label">Classes Enrolled</div>
            <div class="mini-stat__value">{{ card.classes.length }}</div>
          </div>
        </div>
        <div class="mini-stat">
          <div class="mini-stat__icon"><q-icon name="o_schedule" size="22px" /></div>
          <div>
            <div class="mini-stat__label">Next Class</div>
            <div class="mini-stat__value mini-stat__value--sm">{{ card.nextClass || "—" }}</div>
          </div>
        </div>
      </div>

      <div class="student-card__enrolled">
        <h5 class="enrolled-head">Enrolled Classes</h5>
        <div v-if="!card.classes.length" class="empty-card empty-card--inline">
          Not enrolled in any classes yet.
        </div>
        <div v-else class="class-grid">
          <div v-for="cls in card.classes" :key="cls.classId" class="class-card">
            <div class="class-card__top">
              <h5>{{ cls.name }}</h5>
            </div>
            <div class="class-card__detail">
              <span v-if="cls.instructor"><q-icon name="o_person" size="13px" />Instructor: {{ cls.instructor }}</span>
              <span v-if="cls.room"><q-icon name="o_meeting_room" size="13px" />Studio: {{ cls.room }}</span>
              <span v-if="cls.schedule"><q-icon name="o_calendar_today" size="13px" />{{ cls.schedule }}</span>
            </div>
            <div class="class-card__foot">
              <div class="class-card__next">Next: {{ cls.next || "—" }}</div>
              <q-btn
                unelevated no-caps dense
                color="primary"
                label="View Schedule"
                class="class-card__btn"
                @click="viewSchedule"
              />
            </div>
          </div>
        </div>
      </div>
    </div>

    <view-student :id="viewingId" v-model="viewOpen" />
    <!-- The same student form the admin side uses, in Add mode. -->
    <student-edit-dialog v-model="addOpen" :family-id="addFamilyId" @saved="load" />
  </q-page>
</template>

<script setup>
import { computed, onMounted, ref } from "vue";
import { useRouter } from "vue-router";
import { Permissions, usePermissions } from "composables/usePermissions";
import { useNotify } from "composables/useNotify";
import {
  formatClassSchedule, nextSessionOf, relativeDay, shortTime, studentFullName, toMinutes, useParentPortalData
} from "composables/useParentPortal";
import ViewStudent from "modules/student/components/view_student.vue";
import StudentEditDialog from "modules/family/components/StudentEditDialog.vue";

const router = useRouter();
const notify = useNotify();
const { has } = usePermissions();
const { loading, error, students, classesOf, roomOf, instructorOf, load } = useParentPortalData();

const initialsOf = (s) =>
  [s.firstName, s.lastName].filter(Boolean).map((part) => part.trim()[0]?.toUpperCase()).join("") || "?";

const ageOf = (birthDate) => {
  if (!birthDate) return "";
  const born = new Date(birthDate);
  if (Number.isNaN(born.getTime())) return "";
  const now = new Date();
  let years = now.getFullYear() - born.getFullYear();
  if (now.getMonth() < born.getMonth() || (now.getMonth() === born.getMonth() && now.getDate() < born.getDate())) years--;
  return years >= 0 ? `${years} Year${years === 1 ? "" : "s"}` : "";
};

const cards = computed(() =>
  [...students.value]
    .sort((a, b) => studentFullName(a).localeCompare(studentFullName(b)))
    .map((s) => {
      const classes = classesOf(s).map((cls) => ({ cls, session: nextSessionOf(cls) }));

      // Soonest upcoming session across this student's classes → "Tomorrow, 4:30 PM".
      const soonest = classes
        .filter((c) => c.session)
        .sort((a, b) => a.session.offset - b.session.offset ||
          (toMinutes(a.cls.startTime) ?? 9999) - (toMinutes(b.cls.startTime) ?? 9999))[0];

      return {
        studentId: s.studentId,
        name: studentFullName(s),
        initials: initialsOf(s),
        active: s.active,
        age: ageOf(s.birthDate),
        gradeLevel: s.gradeLevel || "",
        nextClass: soonest
          ? [relativeDay(soonest.session), shortTime(soonest.cls.startTime)].filter(Boolean).join(", ")
          : "",
        classes: classes.map(({ cls, session }) => ({
          classId: cls.classId,
          name: cls.className || "Class",
          instructor: instructorOf(cls),
          room: roomOf(cls),
          schedule: formatClassSchedule(cls),
          next: session ? relativeDay(session, { longDate: true }) : ""
        }))
      };
    }));

const viewSchedule = () =>
  (has(Permissions.ClassesRead) ? router.push("/classes") : notify.info("The schedule isn't available yet."));

// ---- View student (the existing read-only drawer) ----
const viewOpen = ref(false);
const viewingId = ref(null);
const openView = (studentId) => {
  viewingId.value = studentId;
  viewOpen.value = true;
};

// ---- Add student ----
// The API only lets a family contact add a student to their own family (StudentsController.CheckOwnFamiliesAsync),
// and the family list isn't scoped to the contact — so the family is taken from the contact's existing students.
const addOpen = ref(false);
const addFamilyId = ref(null);
const openAdd = () => {
  if (!has(Permissions.StudentsWrite)) {
    notify.warning("Your account isn't allowed to add students. Please contact the academy.");
    return;
  }
  addFamilyId.value = students.value.find((s) => s.familyId)?.familyId || null;
  if (!addFamilyId.value) {
    notify.warning("Your family record couldn't be found. Please contact the academy to add a student.");
    return;
  }
  addOpen.value = true;
};

onMounted(load);
</script>

<style scoped>
.parent-students {
  display: flex;
  flex-direction: column;
  gap: 32px;
}

/* ---- Header ---- */
.parent-students__header {
  display: flex;
  align-items: flex-end;
  justify-content: space-between;
  flex-wrap: wrap;
  gap: 24px;
}

.parent-students__title {
  font-size: 40px;
  font-weight: 700;
  letter-spacing: -0.8px;
  line-height: 1.2;
  margin: 6px 0 0;
  color: var(--on-surface);
}

.parent-students__subtitle {
  font-size: 16px;
  color: var(--on-surface-variant);
  margin: 6px 0 0;
  max-width: 600px;
}

.parent-students__add {
  border-radius: 12px;
  padding: 10px 24px;
  font-weight: 600;
  letter-spacing: 0.7px;
  box-shadow: 0 10px 15px -3px rgba(70, 72, 212, 0.2), 0 4px 6px -4px rgba(70, 72, 212, 0.2);
}

/* ---- Student card ---- */
.student-card {
  background: var(--surface);
  border: 1px solid var(--outline-variant);
  border-radius: 16px;
  box-shadow: 0 4px 20px rgba(15, 23, 42, 0.05);
  overflow: hidden;
}

.student-card__head {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: 24px;
  padding: 24px;
  border-bottom: 1px solid var(--outline-variant);
}

.student-card__photo {
  width: 96px;
  height: 96px;
  flex-shrink: 0;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 16px;
  border: 4px solid #fff;
  box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1), 0 2px 4px -2px rgba(0, 0, 0, 0.1);
  background: var(--surface-container);
  color: var(--primary);
  font-size: 32px;
  font-weight: 700;
}

.student-card__identity { flex: 1; min-width: 200px; }

.student-card__name-row {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: 16px;
}

.student-card__name-row h4 {
  margin: 0;
  font-size: 24px;
  font-weight: 600;
  line-height: 1.3;
  color: var(--on-surface);
}

.student-card__meta {
  display: flex;
  flex-wrap: wrap;
  gap: 40px;
  margin-top: 6px;
  font-size: 16px;
  color: var(--on-surface-variant);
}

.student-card__meta span { display: flex; align-items: center; gap: 6px; }

.icon-round {
  width: 40px;
  height: 40px;
  flex-shrink: 0;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 99px;
  background: var(--surface-container-low);
  border: 1px solid rgba(199, 196, 215, 0.3);
  color: var(--primary);
  cursor: pointer;
}

.badge {
  display: inline-flex;
  align-items: center;
  padding: 4px 14px;
  border-radius: 99px;
  font-size: 11px;
  font-weight: 700;
  letter-spacing: 0.5px;
  text-transform: uppercase;
}

.badge--success { background: #dcfce7; color: #15803d; }
.badge--neutral { background: var(--surface-container-low); color: var(--outline); }

/* ---- Mini stats ---- */
.student-card__stats {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 24px;
  padding: 24px;
}

.mini-stat {
  display: flex;
  align-items: center;
  gap: 16px;
  padding: 17px;
  background: var(--surface-container-low);
  border: 1px solid rgba(199, 196, 215, 0.3);
  border-radius: 12px;
}

.mini-stat__icon {
  width: 48px;
  height: 48px;
  flex-shrink: 0;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 8px;
  background: rgba(70, 72, 212, 0.1);
  color: var(--primary);
}

.mini-stat__label { font-size: 12px; letter-spacing: 0.24px; color: var(--on-surface-variant); }
.mini-stat__value { font-size: 24px; font-weight: 700; color: var(--on-surface); }
.mini-stat__value--sm { font-size: 16px; }

/* ---- Enrolled classes ---- */
.student-card__enrolled { padding: 0 24px 24px; }

.enrolled-head {
  margin: 0 0 20px;
  padding-left: 20px;
  border-left: 4px solid var(--primary);
  font-size: 20px;
  font-weight: 400;
  line-height: 1.4;
  color: var(--on-surface);
}

.class-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 24px;
}

.class-card {
  display: flex;
  flex-direction: column;
  gap: 16px;
  padding: 25px;
  background: var(--surface-container-lowest);
  border: 1px solid var(--outline-variant);
  border-radius: 12px;
}

.class-card__top {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  gap: 8px;
}

.class-card__top h5 {
  margin: 0;
  font-size: 18px;
  font-weight: 400;
  line-height: 1.4;
  color: var(--on-surface);
}

.class-card__detail {
  display: flex;
  flex-direction: column;
  gap: 8px;
  padding: 12px 0;
  border-top: 1px solid rgba(199, 196, 215, 0.3);
  border-bottom: 1px solid rgba(199, 196, 215, 0.3);
  font-size: 14px;
  color: var(--on-surface-variant);
}

.class-card__detail span { display: flex; align-items: center; gap: 8px; }

.class-card__foot {
  display: flex;
  align-items: center;
  justify-content: space-between;
  flex-wrap: wrap;
  gap: 8px;
  margin-top: auto;
}

.class-card__next { font-size: 12px; letter-spacing: 0.24px; color: var(--on-surface-variant); }

.class-card__btn {
  border-radius: 8px;
  padding: 4px 16px;
  font-size: 13px;
  font-weight: 600;
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

.empty-card--inline { padding: 16px; border-radius: 12px; }

@media (max-width: 1000px) {
  .class-grid { grid-template-columns: 1fr; }
}

@media (max-width: 600px) {
  .parent-students__title { font-size: 30px; }
  .student-card__stats { grid-template-columns: 1fr; }
}
</style>
