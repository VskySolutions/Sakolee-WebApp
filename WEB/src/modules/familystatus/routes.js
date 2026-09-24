export default [
  {
    path: "/familystatus",
    component: () => import("layouts/layout.vue"),
    children: [
      {
        path: "",
        name: "family_statuses",
        component: () => import("modules/familystatus/pages/index.vue"),
        meta: { requiresAuth: true, permissions: ["familyStatuses.read"], title: "Family Statuses" }
      },
      {
        path: "create",
        name: "family_status_create",
        component: () => import("modules/familystatus/components/create_edit_status.vue"),
        meta: { requiresAuth: true, permissions: ["familyStatuses.write"], title: "Create Family Status" }
      },
      {
        path: ":id/edit",
        name: "family_status_edit",
        component: () => import("modules/familystatus/components/create_edit_status.vue"),
        meta: { requiresAuth: true, permissions: ["familyStatuses.write"], title: "Edit Family Status" }
      },
      {
        path: ":id",
        name: "family_status_detail",
        component: () => import("modules/familystatus/components/viewstatus.vue"),
        meta: { requiresAuth: true, permissions: ["familyStatuses.read"], title: "Family Status Details" }
      }
    ]
  }
];
