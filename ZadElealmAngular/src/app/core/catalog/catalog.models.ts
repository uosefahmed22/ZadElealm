import { ApiResponseEnvelope } from '../api/api-error.models';

export interface CategoryDto {
  id: number;
  name: string;
  description: string;
  imageUrl: string;
}

export interface CourseDto {
  id: number;
  name: string;
  description: string;
  author: string;
  courseLanguage: string;
  courseVideosCount: number;
  rating: number;
  imageUrl: string | null;
  category: CategoryDto | null;
  createdAt: string;
}

export interface PaginationMetadata {
  pageSize: number;
  currentPage: number;
  totalMatchedItems: number;
  nextPage: number | null;
  previousPage: number | null;
  numberOfPages: number;
}

export interface PaginatedCoursesResponse extends ApiResponseEnvelope {
  data: CourseDto[];
  metaData: PaginationMetadata;
}

export interface CourseCatalogFilters {
  categoryId: number;
  search: string;
  author: string;
  language: string;
  minRating: number;
  sortBy: 'date' | 'rating' | 'name';
  sortDirection: 'asc' | 'desc';
  pageNumber: number;
  pageSize: number;
}
