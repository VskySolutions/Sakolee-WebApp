<template>
  <q-card flat bordered class="panel">
    <!-- Header: title + caption on the left, the "see everything" link on the right. -->
    <q-card-section class="row items-start no-wrap q-pb-sm">
      <div class="col">
        <div class="panel__title">Enrollment &amp; Student Activity</div>
        <div class="panel__caption">Live status changes</div>
      </div>
      <q-btn flat dense no-caps size="sm" color="primary" class="panel__link" icon-right="o_chevron_right" label="View All" :to="{ path: '/students' }" />
    </q-card-section>

    <q-card-section class="q-pt-none">
      <div class="panel__group-label">
        <span class="panel__dot" />Recently Enrolled
      </div>

      <div v-if="!rows.length" class="text-grey-6 text-center q-pa-md">No recent enrollments.</div>

      <q-markup-table v-else flat dense class="activity-table">
        <thead>
          <tr>
            <th class="text-left">Student</th>
            <th class="text-left">Family</th>
            <th class="text-left">Class</th>
            <th class="text-right">Status</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="row in rows" :key="row.id">
            <td class="text-left">
              <router-link v-if="row.studentId" class="activity-table__link" :to="{ path: `/students/${row.studentId}` }">{{ row.student }}</router-link>
              <span v-else class="activity-table__link">{{ row.student }}</span>
            </td>
            <td class="text-left">
              <router-link v-if="row.familyId" class="activity-table__link" :to="{ path: `/persons/${row.familyId}` }">{{ row.family }}</router-link>
              <span v-else class="activity-table__link">{{ row.family }}</span>
            </td>
            <td class="text-left activity-table__muted">{{ row.className }}</td>
            <td class="text-right">
              <span class="status-chip" :style="statusStyle(row.status)">{{ row.status }}</span>
            </td>
          </tr>
        </tbody>
      </q-markup-table>
    </q-card-section>
  </q-card>
</template>

<script setup>
import { computed } from "vue";

const props = defineProps({
  // [{ id, student, studentId, family, familyId, className, status }]
  activity: { type: Array, default: () => [] }
});

const rows = computed(() => props.activity || []);

// The prototype draws a confirmed enrolment as emerald-600 on emerald-50; the rest follow the same
// tint/ink pairing from the token set.
const STATUS_TONES = {
  confirmed: { color: "var(--success)", background: "var(--success-tint)" },
  pending: { color: "var(--warning)", background: "var(--warning-tint)" },
  waitlist: { color: "var(--primary)", background: "var(--primary-tint)" },
  dropped: { color: "var(--error)", background: "var(--error-tint)" }
};

const statusStyle = (status) =>
  STATUS_TONES[String(status || "").toLowerCase()] ??
  { color: "var(--outline)", background: "var(--surface-container)" };
</script>

<style scoped>
.panel { border-radius: 16px; height: 100%; }

.panel__title {
  font-size: 15px;
  font-weight: 700;
  color: var(--on-surface);
}

.panel__caption {
  font-size: 12px;
  color: var(--outline);
}

.panel__link { font-weight: 700; font-size: 12px; }

/* Written in sentence case and capitalised here, as the prototype does it — an all-caps literal in
   the markup is read out letter-by-letter by screen readers. */
.panel__group-label {
  display: flex;
  align-items: center;
  gap: 6px;
  font-size: 11px;
  font-weight: 700;
  letter-spacing: 0.06em;
  text-transform: uppercase;
  color: var(--success);
  margin-bottom: 4px;
}

.panel__dot {
  width: 6px;
  height: 6px;
  border-radius: 50%;
  background: currentColor;
}

/* Quiet, uppercase headers and hairline rows — the table is a reading surface, not a data grid. */
.activity-table :deep(thead th) {
  font-size: 10px;
  font-weight: 700;
  letter-spacing: 0.06em;
  text-transform: uppercase;
  color: var(--outline);
  border-bottom: 1px solid var(--line);
}

.activity-table :deep(tbody td) {
  font-size: 13px;
  border-bottom: 1px solid var(--line);
}

.activity-table :deep(tbody tr:last-child td) { border-bottom: none; }

.activity-table__muted { color: var(--on-surface-variant); }

.activity-table__link {
  color: var(--primary);
  font-weight: 600;
  text-decoration: none;
}

.activity-table__link:hover { text-decoration: underline; }

.status-chip {
  display: inline-block;
  font-size: 11px;
  font-weight: 600;
  border-radius: 6px;
  padding: 3px 9px;
  white-space: nowrap;
}
</style>
