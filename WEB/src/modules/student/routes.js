export default [
  {
    path: "/students",
    component: () => import("layouts/layout.vue"),
    children: [
      {
        path: "",
        name: "students",
        component: () => import("modules/student/pages/index.vue"),
        meta: { requiresAuth: true, permissions: ["students.read"], title: "Student" }
      }
    ]
  }
];
