export default [
  {
    path: "/account-type",
    component: () => import("layouts/layout.vue"),

    children: [
      {
        path: "",
        name: "account_type",
        component: () =>
          import("modules/account-type/pages/index.vue"),

        meta: {
          requiresAuth: true,
          permissions: ["accountTypes.read"],
          title: "Account Type"
        }
      }
    ]
  }
];
