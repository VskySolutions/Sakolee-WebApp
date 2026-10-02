import { ref, unref, watch, onMounted } from "vue";
import { dashboardApi, getApiErrorMessage } from "services/api";

// Studio dashboard data (Dashboard Overview).
//
// The Enrollment & Studio Metrics band (`GET /api/dashboard/studio-metrics`) and the Enrollment &
// Student Activity feed (`GET /api/dashboard/enrollment-activity`) are live. The rest of the studio
// aggregate has no endpoint yet, so it still resolves the placeholder payload below.
// It is shaped exactly as the endpoint's response is expected to be, so wiring it up is a
// one-line swap:
//
//   const data = await dashboardApi.studio(paramsFrom(dateRange));
//
// in place of the `resolvePlaceholder` call, plus a `getApiErrorMessage(err)` in the catch.

const PLACEHOLDER = {
  // Severity drives the card's accent: "critical" | "warning" | "info".
  alerts: [
    {
      id: "a1",
      severity: "critical",
      title: "3 Attendance discrepancies requiring review",
      detail: "Ballet Basic Level 2 & 4",
      footnote: "Action required",
      actionLabel: "Resolve",
      actionRoute: { path: "/classes" }
    },
    {
      id: "a2",
      severity: "warning",
      title: "Advanced Ballet II has 5 students on waitlist",
      detail: "Coach Marcus CPR expiring in 5 days",
      footnote: "Expires soon",
      actionLabel: "View Staff",
      actionRoute: { path: "/persons" }
    },
    {
      id: "a3",
      severity: "info",
      title: "Spring Recital 2026 window open",
      detail: "Portal Submissions Active",
      footnote: "Maintenance Sun 2 AM",
      actionLabel: "Configure",
      actionRoute: { path: "/classes" }
    }
  ],

  // Active alert count for the section badge — deliberately separate from `alerts.length`,
  // since the strip shows only the three most pressing.
  activeAlertCount: 5,

  updatedAt: "Updated 4 mins ago",

  // Priority drives the label on the right: "urgent" | "medium" | "low".
  tasks: [
    { id: "t1", title: "Finalize costume orders", due: "Today", owner: "Sarah J.", priority: "urgent" },
    { id: "t2", title: "Recital rehearsal schedule", due: "Tomorrow", owner: "Sarah J.", priority: "medium" },
    { id: "t3", title: "Review waitlist for Advanced Ballet II", due: "Fri", owner: "Marcus T.", priority: "medium" },
    { id: "t4", title: "Approve portal registrations", due: "Next week", owner: "Sarah J.", priority: "low" }
  ],

  // Receivables, oldest-debt-last. `severity` orders the buckets; "none" is money that is not yet
  // due and so carries no severity mark.
  receivables: {
    totalOverdue: 25830,
    buckets: [
      { key: "current", label: "Current / Not Yet Overdue", amount: 18450, severity: "none" },
      { key: "d1to30", label: "1 - 30 Days Overdue", amount: 4250, severity: "low" },
      { key: "d31to60", label: "31 - 60 Days Overdue", amount: 1890, severity: "medium" },
      { key: "d60plus", label: "60+ Days Overdue", amount: 1240, severity: "high" }
    ]
  },

  // Twelve months of billed revenue. `ytdTotal` is the sum of `points`; `changePct` compares it to
  // the same period last year.
  revenue: {
    ytdTotal: 124500,
    changePct: 12.4,
    points: [
      { label: "Jan", value: 7900 },
      { label: "Feb", value: 8300 },
      { label: "Mar", value: 8800 },
      { label: "Apr", value: 9200 },
      { label: "May", value: 9600 },
      { label: "Jun", value: 10100 },
      { label: "Jul", value: 10500 },
      { label: "Aug", value: 11000 },
      { label: "Sep", value: 11400 },
      { label: "Oct", value: 11900 },
      { label: "Nov", value: 12400 },
      { label: "Dec", value: 13400 }
    ]
  },

  announcements: [
    {
      id: "n1",
      title: "Spring Recital Costumes Ordering Deadline",
      body: "All size charts for Level 1 through Senior ensemble must be submitted by Friday.",
      postedOn: "2026-09-18"
    },
    {
      id: "n2",
      title: "Studio Closed — Staff Training Day",
      body: "North Studio is closed Monday for annual safeguarding and first-aid training.",
      postedOn: "2026-09-15"
    }
  ]
};

// Stands in for the network call, so `loading` behaves the way it will once the endpoint lands.
const resolvePlaceholder = () =>
  new Promise((resolve) => setTimeout(() => resolve(structuredClone(PLACEHOLDER)), 250));

export function useStudioDashboard (dateRange) {
  const loading = ref(false);
  const error = ref(null);

  const alerts = ref([]);
  const activeAlertCount = ref(0);
  const metrics = ref(null);
  const termLabel = ref("");
  const updatedAt = ref("");
  const recentActivity = ref([]);
  const tasks = ref([]);
  const receivables = ref(null);
  const revenue = ref(null);
  const announcements = ref([]);

  const refresh = async () => {
    loading.value = true;
    error.value = null;
    try {
      const [data, liveMetrics, liveActivity] = await Promise.all([
        resolvePlaceholder(),
        dashboardApi.studioMetrics(),
        dashboardApi.enrollmentActivity({ limit: 5 })
      ]);
      alerts.value = data?.alerts ?? [];
      activeAlertCount.value = data?.activeAlertCount ?? (data?.alerts?.length ?? 0);
      metrics.value = liveMetrics ?? null;
      termLabel.value = `As of ${new Date().toLocaleString(undefined, { dateStyle: "medium", timeStyle: "short" })}`;
      updatedAt.value = data?.updatedAt ?? "";
      recentActivity.value = liveActivity ?? [];
      tasks.value = data?.tasks ?? [];
      receivables.value = data?.receivables ?? null;
      revenue.value = data?.revenue ?? null;
      announcements.value = data?.announcements ?? [];
    } catch (err) {
      error.value = getApiErrorMessage(err, "Unable to load the dashboard.");
    } finally {
      loading.value = false;
    }
  };

  watch(() => unref(dateRange), refresh);
  onMounted(refresh);

  return {
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
  };
}
