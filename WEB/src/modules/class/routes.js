export default [
  {
    path: "/classes",
    component: () => import("layouts/layout.vue"),
    children: [
      {
        path: "",
        name: "classes",
        component: () => import("modules/class/pages/index.vue"),
        meta: { requiresAuth: true, permissions: ["classes.read"], title: "Classes" }
      }
    ]
  }
];
