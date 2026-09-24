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
        // Part of the Families area: a role without families.read sees neither this nor All Families.
        component: () => import("modules/family/pages/quick_registration.vue"),
        meta: { requiresAuth: true, permissions: ["families.read"], title: "Quick Registration" }
      }
    ]
  }
];
