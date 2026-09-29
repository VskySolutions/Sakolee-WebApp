export default [
  {
    path: "/billing-methods",
    component: () => import("layouts/layout.vue"),
    children: [
      // Route for listing all bank methods (Index Page)
      {
        path: "",
        name: "billing_methods",
        component: () => import("modules/billing-method/pages/index.vue"),
        meta: { requiresAuth: true, title: "Billing Methods" }
      },
      // Route for creating a new bank method record
      {
        path: "create",
        name: "billing_method_create",
        component: () => import("modules/billing-method/components/create_edit.vue"),
        meta: { requiresAuth: true, title: "Create Billing Method" }
      },
      // Route for viewing detailed information of a specific bank method record
      {
        path: ":id",
        name: "billing_method_detail",
        component: () => import("modules/billing-method/components/view.vue"),
        meta: { requiresAuth: true, title: "Billing Method Details" }
      }
    ]
  }
];