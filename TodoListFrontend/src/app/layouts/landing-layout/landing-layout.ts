import { Component } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';

@Component({
    selector: 'app-landing-layout',
    imports: [RouterOutlet, RouterLink, RouterLinkActive],
    templateUrl: './landing-layout.html',
    styleUrl: './landing-layout.css'
})
export class LandingLayoutComponent { }
