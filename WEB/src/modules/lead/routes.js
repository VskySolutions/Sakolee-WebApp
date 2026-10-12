export default [
  {
    path: "/families/leads",
    component: () => import("layouts/layout.vue"),
    children: [
      {
        path: "",
        name: "leads",
        component: () => import("modules/lead/pages/index.vue"),
        // Part of the Families area: a role without families.read sees neither this nor All Families.
        meta: { requiresAuth: true, permissions: ["families.read"], title: "Lead Files" }
      }
    ]
  }
];
