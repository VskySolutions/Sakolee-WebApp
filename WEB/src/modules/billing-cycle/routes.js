export default [
  {
    path: "/billing-cycles",
    component: () => import("layouts/layout.vue"),
    children: [
      {
        path: "",
        name: "billing_cycles",
        component: () =>
          import("modules/billing-cycle/pages/index.vue"),
        meta: {
          requiresAuth: true,
          permissions: ["billingCycles.read"],
          title: "Billing Cycles"
        }
      }
    ]
  }
];
