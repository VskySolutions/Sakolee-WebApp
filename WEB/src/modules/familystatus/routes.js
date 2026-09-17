export default [
  {
    path: "/familystatus",
    component: () => import("layouts/layout.vue"),
    children: [
      {
        path: "",
        name: "family_statuses",
        component: () => import("modules/familystatus/pages/index.vue"),
        // meta: { requiresAuth: true, permissions: ["family_status.manage"], title: "Family Statuses" }
        meta: { requiresAuth: true, title: "Family Statuses" }
      },
      {
        path: "create",
        name: "family_status_create",
        component: () => import("modules/familystatus/components/create_edit_status.vue"),
        meta: { requiresAuth: true, title: "Create Family Status" }
      },
      {
        path: ":id/edit",
        name: "family_status_edit",
        component: () => import("modules/familystatus/components/create_edit_status.vue"),
        meta: { requiresAuth: true,title: "Edit Family Status" }
      },
      {
        path: ":id",
        name: "family_status_detail",
        component: () => import("modules/familystatus/components/viewstatus.vue"),
        meta: { requiresAuth: true, title: "Family Status Details" }
      }
    ]
  }
];