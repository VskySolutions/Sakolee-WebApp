export default [
  {
    path: "/families",
    component: () => import("layouts/layout.vue"),
    children: [
      {
        path: "quick-registration",
        name: "family_quick_registration",
        // Ungated: there is no family permission in the catalogue yet, so requiring one here would
        // shut every role out of the page.
        component: () => import("modules/family/pages/quick_registration.vue"),
        meta: { requiresAuth: true, title: "Quick Registration" }
      }
    ]
  }
];
