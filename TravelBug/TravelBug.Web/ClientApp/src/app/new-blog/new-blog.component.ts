import { Component, OnDestroy, OnInit } from "@angular/core";
import { NgForm } from "@angular/forms";
import { ActivatedRoute, Router } from "@angular/router";
import { Blog } from "../models/blog";
import { BlogService } from "../services/blog.service";
import { RouterTrackingService } from "../services/router-tracking.service";

@Component({
  selector: "app-new-blog",
  templateUrl: "./new-blog.component.html",
  styleUrls: ["./new-blog.component.css"],
})
export class NewBlogComponent implements OnInit, OnDestroy {
  blog: Blog;
  warning: string = null;
  backToLink = "/";

  // Whether this is new blog or edited blog
  // Only allowed to edit blog if user is author
  newBlog = true;

  constructor(
    private blogService: BlogService,
    private activatedRoute: ActivatedRoute,
    private router: Router,
    private routerTrackingService: RouterTrackingService
  ) {}

  ngOnInit() {
    let segment = this.activatedRoute.snapshot.url[0].path;
    console.log(segment);
    this.newBlog = segment === "new-blog";

    if (this.newBlog) {
      this.blog = this.blogService.loadCurrentBlog();
    } else {
      this.blog = this.blogService.loadEditedBlog();
    }
    // if (!this.update) this.blog = this.blogService.loadCurrentBlog();
    // else this.blog = new Blog();
    this.backToLink = this.routerTrackingService.prevUrl;
  }

  onSubmit(title: NgForm, description: NgForm) {
    // console.log(this.blog);
    // console.log(title.value, description.value);
    if (!title.value || !description.value) {
      this.warning = "Title and description must be non-empty.";
    } else if (this.newBlog) {
      this.blogService.postBlog(this.blog).subscribe(() => {
        this.blog = new Blog();
        this.router.navigate(["/profile"]);
      });
    } else {
      this.blogService.patchBlog(this.blog).subscribe(() => {
        this.blog = new Blog();
        this.router.navigate(["/profile"]);
      });
    }
  }

  ngOnDestroy() {
    // Save the current blog
    if (this.newBlog) this.blogService.saveCurrentBlog(this.blog);
  }
}
