export default [
  {
    path: "/hear-about-us",
    component: () => import("layouts/layout.vue"),
    children: [
      {
        path: "",
        name: "hear_about_us",
        component: () =>
          import("modules/hear-about-us/pages/index.vue"),
        meta: {
          requiresAuth: true,
          permissions: ["hearAboutUs.read"],
          title: "Hear About Us"
        }
      }
    ]
  }
];
