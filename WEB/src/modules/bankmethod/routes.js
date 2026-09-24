export default [
  {
    path: "/billing-methods",
    component: () => import("layouts/layout.vue"),
    children: [
      // Route for listing all bank methods (Index Page)
      {
        path: "",
        name: "bank_methods",
        component: () => import("modules/bankmethod/pages/index.vue"),
        meta: { requiresAuth: true, title: "Bank Methods" }
      },
      // Route for creating a new bank method record
      {
        path: "create",
        name: "bank_method_create",
        component: () => import("modules/bankmethod/components/create_edit.vue"),
        meta: { requiresAuth: true, title: "Create Bank Method" }
      },
      // Route for viewing detailed information of a specific bank method record
      {
        path: ":id",
        name: "bank_method_detail",
        component: () => import("modules/bankmethod/components/view_bankmethod.vue"),
        meta: { requiresAuth: true, title: "Bank Method Details" }
      }
    ]
  }
];