export default [
  {
    path: "/policies",
    component: () => import("layouts/layout.vue"),
    children: [
      {
        path: "",
        name: "policies",
        component: () => import("modules/policy/pages/index.vue"),
        meta: {
          requiresAuth: true,
          permissions: ["policies.read"],
          title: "Policies"
        }
      }
    ]
  }
];
