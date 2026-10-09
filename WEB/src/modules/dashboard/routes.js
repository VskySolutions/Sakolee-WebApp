export default [
  {
    path: "/dashboard",
    component: () => import("layouts/layout.vue"),
    children: [
      {
        path: "",
        name: "dashboard",
        component: () => import("modules/dashboard/pages/DashboardHome.vue"),
        // Visible to every authenticated user; DashboardHome picks the parent-portal or studio dashboard by role.
        meta: { requiresAuth: true, title: "Dashboard" }
      }
    ]
  }
];
