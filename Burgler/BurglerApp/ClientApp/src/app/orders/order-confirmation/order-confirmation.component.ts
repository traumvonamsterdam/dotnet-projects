import { Component, OnInit, Input, OnDestroy } from "@angular/core";
import { OrderConfirmationService } from "./order-confirmation.service";
import { Subscription } from "rxjs";
import { Order, FoodItem } from "../order";
import { OrderService } from "../order.service";

@Component({
  selector: "app-order-confirmation",
  templateUrl: "./order-confirmation.component.html",
  styleUrls: ["./order-confirmation.component.css"],
})
export class OrderConfirmationComponent implements OnInit, OnDestroy {
  orderConfirmationSub: Subscription;
  order: Order;

  foodItems: FoodItem[];
  price: number;
  pickupTime: Date;
  mode: string;

  titleText: { [key: string]: string } = {
    placeOrder: "Place Order",
    cancelOrder: "Confirm Order Cancellation",
    reorder: "Confirm to Reorder",
  };

  message: { [key: string]: string } = {
    placeOrder: "Are you sure you want to place this order?",
    cancelOrder: "Are you sure you want to cancel this order?",
    reorder: "This will overwrite your current order. Do you want to continue?",
  };

  buttonText: { [key: string]: string } = {
    placeOrder: "Confirm",
    cancelOrder: "Cancel Order",
    reorder: "Confirm",
  };

  constructor(
    private orderConfirmationService: OrderConfirmationService,
    private orderService: OrderService
  ) {}

  ngOnInit() {
    this.orderConfirmationSub = this.orderConfirmationService.orderConfirmationSubject.subscribe(
      ({ order, mode }) => {
        this.order = order;
        this.foodItems = [].concat(
          order.burgerItems,
          order.sideItems,
          order.drinkItems
        );
        this.price = +order.totalPrice;
        this.mode = mode;
        this.pickupTime = order.pickupTime
      }
    );
  }

  onConfirm() {
    if (this.mode === "reorder") {
      this.orderService.reorder(this.order).subscribe();
    } else {
      const changeOrderStatus: { [key: string]: string } = {
        placeOrder: "placeOrder",
        cancelOrder: "cancel",
      };

      this.orderService
        .changeOrderStatus(this.order.orderId, changeOrderStatus[this.mode])
        .subscribe(() => location.reload());
    }
  }

  ngOnDestroy() {
    this.orderConfirmationSub.unsubscribe();
  }
}
