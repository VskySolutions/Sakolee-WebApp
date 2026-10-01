export default [
  {
    path: "/e-payment-schedule",
    component: () => import("layouts/layout.vue"),
    children: [
      {
        path: "",
        name: "e_payment_schedule",
        component: () =>
          import("modules/e-payment-schedule/pages/index.vue"),
        meta: {
          requiresAuth: true,
          permissions: ["ePaymentSchedules.read"],
          title: "E-Payment Schedule"
        }
      }
    ]
  }
];
