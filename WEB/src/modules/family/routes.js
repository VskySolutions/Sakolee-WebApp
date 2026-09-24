export default [
  {
    path: "/families",
    component: () => import("layouts/layout.vue"),
    children: [
      {
        path: "",
        name: "families",
        component: () => import("modules/family/pages/index.vue"),
        meta: { requiresAuth: true, permissions: ["families.read"], title: "Families" }
      },
      {
        path: "quick-registration",
        name: "family_quick_registration",
        // Ungated: Quick Registration collects a family, its contacts, and a student in one pass —
        // any role reaching this page can register a new family, not just one with families.write.
        component: () => import("modules/family/pages/quick_registration.vue"),
        meta: { requiresAuth: true, title: "Quick Registration" }
      }
    ]
  }
];
