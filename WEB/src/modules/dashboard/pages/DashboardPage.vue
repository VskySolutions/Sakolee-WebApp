<template>
  <q-page padding class="dashboard-overview">
    <!-- Page heading: crumbs, then the title block. -->
    <!-- <app-breadcrumbs :items="[{ label: 'Home', to: '/' }, { label: 'Dashboard' }]" no-margin class="dashboard-overview__crumbs" /> -->

    <q-breadcrumbs class="text-brown">
      <template #separator>
        <q-icon size="1.0em" name="o_chevron_right" color="primary" />
      </template>

      <q-breadcrumbs-el label="Home" class="text-86 fw-700 text-uppercase sfs-11" />
      <q-breadcrumbs-el label="Dashboard" class="text-primary fw-700 text-uppercase sfs-11" />
    </q-breadcrumbs>

    <h1 class="dashboard-overview__title">Dashboard</h1>
    <div class="dashboard-overview__subtitle q-mb-lg text-54">
      Quick overview of your studio's daily activities, student activity, alerts and business performance.
    </div>

    <q-banner v-if="error" dense rounded class="bg-red-1 text-negative q-mb-md">
      <template #avatar><q-icon name="o_error" color="negative" /></template>
      {{ error }}
      <template #action>
        <q-btn flat dense no-caps color="negative" label="Retry" @click="refresh" />
      </template>
    </q-banner>

    <!-- ---- Alerts & Attention Required ---- -->
    <studio-section-header
      title="Alerts & Attention Required"
      icon="o_error"
      icon-color="negative"
      :badge="activeAlertCount ? `${activeAlertCount} ACTIVE` : null"
      :note="updatedAt"
    />

    <div class="row q-col-gutter-md q-mb-lg">
      <template v-if="loading">
        <div v-for="n in 3" :key="`alert-skeleton-${n}`" class="col-12 col-md-4">
          <q-skeleton type="rect" height="132px" class="dashboard-overview__skeleton" />
        </div>
      </template>
      <div v-for="alert in alerts" v-else :key="alert.id" class="col-12 col-md-4">
        <studio-alert-card :alert="alert" />
      </div>
    </div>

    <!-- ---- Enrollment & Studio Metrics ---- -->
    <studio-section-header
      title="Enrollment & Studio Metrics"
      icon="o_grid_view"
      :note="termLabel"
    />

    <div class="row q-col-gutter-md q-mb-lg">
      <template v-if="loading">
        <div v-for="n in METRIC_CARDS.length" :key="`metric-skeleton-${n}`" class="col-12 col-sm-6 col-md-4">
          <q-skeleton type="rect" height="88px" class="dashboard-overview__skeleton" />
        </div>
      </template>
      <div v-for="card in metricCards" v-else :key="card.key" class="col-12 col-sm-6 col-md-4">
        <studio-metric-card
          :label="card.label"
          :value="card.value"
          :icon="card.icon"
          :tone="card.tone"
          :to="card.to"
        />
      </div>
    </div>

    <!-- ---- Activity feed alongside the viewer's task list ---- -->
    <div class="row q-col-gutter-md q-mb-lg">
      <div class="col-12 col-lg-8">
        <q-skeleton v-if="loading" type="rect" height="280px" class="dashboard-overview__skeleton" />
        <enrollment-activity-panel v-else :activity="recentActivity" />
      </div>
      <div class="col-12 col-lg-4">
        <q-skeleton v-if="loading" type="rect" height="280px" class="dashboard-overview__skeleton" />
        <my-tasks-panel v-else :tasks="tasks" @add="onAddTask" />
      </div>
    </div>

    <!-- ---- Receivables alongside revenue ---- -->
    <div class="row q-col-gutter-md q-mb-lg">
      <div class="col-12 col-lg-6">
        <q-skeleton v-if="loading" type="rect" height="300px" class="dashboard-overview__skeleton" />
        <unpaid-balances-panel v-else :receivables="receivables" @view-unpaid="onViewUnpaid" />
      </div>
      <div class="col-12 col-lg-6">
        <q-skeleton v-if="loading" type="rect" height="300px" class="dashboard-overview__skeleton" />
        <revenue-analytics-panel v-else :revenue="revenue" />
      </div>
    </div>

    <!-- ---- Announcements ---- -->
    <div class="row q-col-gutter-md">
      <div class="col-8">
        <q-skeleton v-if="loading" type="rect" height="180px" class="dashboard-overview__skeleton" />
        <announcements-panel v-else :announcements="announcements" @post="onPostAnnouncement" />
      </div>
      <div class="col-4">
        <div class="bg-white border border-outline-variant/50 rounded-2xl p-5 shadow-xs">
          <h3 class="text-sm font-bold text-on-surface pb-3 border-b border-outline-variant/30">Quick Links</h3>
          <div class="grid grid-cols-3 gap-2 mt-4 text-center">
            <div class="p-3 bg-surface-container-low rounded-xl hover:bg-surface-container cursor-pointer" @click="$router.push('/families/quick-registration')">
              <span class="material-symbols-outlined fs-22 text-primary">family_restroom</span>
              <span class="block text-[10px] font-semibold mt-1">Families</span>
            </div>
            <div class="p-3 bg-surface-container-low rounded-xl hover:bg-surface-container cursor-pointer" @click="$router.push('/students')">
              <span class="material-symbols-outlined fs-22 text-primary">group</span>
              <span class="block text-[10px] font-semibold mt-1">Students</span>
            </div>
            <div class="p-3 bg-surface-container-low rounded-xl hover:bg-surface-container cursor-pointer" @click="$router.push('/classes')">
              <span class="material-symbols-outlined fs-22 text-primary">school</span>
              <span class="block text-[10px] font-semibold mt-1">Classes</span>
            </div>
          </div>
        </div>
      </div>
    </div>
  </q-page>
</template>

<script setup>
import { computed } from "vue";
import AppBreadcrumbs from "components/common/AppBreadcrumbs.vue";
import StudioSectionHeader from "components/dashboard/studio/StudioSectionHeader.vue";
import StudioAlertCard from "components/dashboard/studio/StudioAlertCard.vue";
import StudioMetricCard from "components/dashboard/studio/StudioMetricCard.vue";
import EnrollmentActivityPanel from "components/dashboard/studio/EnrollmentActivityPanel.vue";
import MyTasksPanel from "components/dashboard/studio/MyTasksPanel.vue";
import UnpaidBalancesPanel from "components/dashboard/studio/UnpaidBalancesPanel.vue";
import RevenueAnalyticsPanel from "components/dashboard/studio/RevenueAnalyticsPanel.vue";
import AnnouncementsPanel from "components/dashboard/studio/AnnouncementsPanel.vue";
import { useStudioDashboard } from "composables/useStudioDashboard";
import { useNotify } from "composables/useNotify";

const notify = useNotify();

const {
  loading,
  error,
  refresh,
  alerts,
  activeAlertCount,
  metrics,
  termLabel,
  updatedAt,
  recentActivity,
  tasks,
  receivables,
  revenue,
  announcements
} = useStudioDashboard();

// The KPI band, in the reference's reading order. `field` maps onto the metrics payload, `tone` is
// the tile hue the prototype gives that card, and `to` is set only where the app already has a list
// page to land on.
const METRIC_CARDS = [
  { key: "totalEnrollments", label: "Total Enrollments", field: "totalEnrollments", icon: "o_assignment_turned_in", tone: "purple", to: { path: "/students" } },
  { key: "recentlyDropped", label: "Recently Dropped", field: "recentlyDropped", icon: "o_person_remove", tone: "red", to: { path: "/students" } },
  { key: "activeFamilies", label: "Active Families", field: "activeFamilies", icon: "o_family_restroom", tone: "indigo", to: { path: "/persons" } },
  { key: "activeStudents", label: "Active Students", field: "activeStudents", icon: "o_group", tone: "blue", to: { path: "/students" } },
  { key: "activeClasses", label: "Active Classes", field: "activeClasses", icon: "o_school", tone: "emerald", to: { path: "/classes" } },
  { key: "activeStaff", label: "Active Staff", field: "activeStaff", icon: "o_badge", tone: "cyan", to: { path: "/persons" } },
  { key: "newOnlineRegistrations", label: "New Online Registrations", field: "newOnlineRegistrations", icon: "o_how_to_reg", tone: "teal", to: null },
  { key: "portalEnrollments", label: "Portal Enrollments", field: "portalEnrollments", icon: "o_touch_app", tone: "amber", to: null },
  { key: "pendingRequests", label: "Pending Requests", field: "pendingRequests", icon: "o_pending_actions", tone: "rose", to: null }
];

const metricCards = computed(() =>
  METRIC_CARDS.map((card) => ({ ...card, value: metrics.value?.[card.field] ?? 0 })));

// Tasks, receivables and announcements have no CRUD endpoints yet, so these acknowledge rather
// than opening a form or navigating somewhere that does not exist.
const onAddTask = () => notify.info("Task creation isn't available yet.");
const onViewUnpaid = () => notify.info("The unpaid families list isn't available yet.");
const onPostAnnouncement = () => notify.info("Posting announcements isn't available yet.");
</script>

<style scoped>
.dashboard-overview__crumbs {
  text-transform: uppercase;
  font-size: 11px;
  letter-spacing: 0.06em;
  font-weight: 600;
}

.dashboard-overview__title {
  font-size: 28px;
  font-weight: 700;
  line-height: 1.2;
  margin: 4px 0 4px;
  color: var(--on-surface);
}

.dashboard-overview__subtitle {
  font-size: 12px;
  color: var(--outline);
}

.dashboard-overview__skeleton { border-radius: 16px; }

.panel { border-radius: 16px; height: 100%; }

.panel__title {
  font-size: 14px;
  font-weight: 700;
  color: var(--on-surface);
}

.panel__action { font-weight: 600; font-size: 12px; }

.note {
  background: var(--surface-container-low);
  border: 1px solid var(--line);
  border-radius: 12px;
  padding: 10px 12px;
}

.note__title {
  font-size: 12px;
  font-weight: 700;
  color: var(--on-surface);
}

.note__date {
  font-size: 11px;
  color: var(--outline);
  margin-left: 12px;
  white-space: nowrap;
}

.note__body {
  font-size: 11px;
  color: var(--on-surface-variant);
  margin: 2px 0 0;
}
</style>
