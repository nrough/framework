#' Minimalistic, beautiful ggplot2 theme
#'
#' A ggplot2 plotting theme that removes a lot of the clutter present in
#' theme_grey.
#' 
#' This is an extended version of what was proposed by Max Woolf in his blog:
#' \link{http://minimaxir.com/2015/02/ggplot-tutorial/}
#' 
#' @param for_print If this is true, the background color is set to plain white
#'   (instead of light grey) and plot titles are omitted.
#' @param tiny 
#' @param heatmap
#' @import grid
#' @import ggplot2
#' @import RColorBrewer
#' @author Eike Petersen
ep_theme <- function(for_print = FALSE, tiny = FALSE, heatmap = FALSE) {
  
  ## Generate the colors for the chart procedurally with RColorBrewer
  palette <- RColorBrewer::brewer.pal("Greys", n=9)
  color.grid.major <- palette[3]
  color.axis.text <- palette[6]
  color.axis.title <- palette[7]
  color.title <- palette[9]
  
  if (for_print) {
    color.background <- palette[1]
    title_element <- ggplot2::element_blank()
    
  } else {
    color.background = palette[2]
    title_element <- ggplot2::element_text(color = color.title,
                                           size = 12,
                                           vjust = 1.25)        
  }
  
  if (heatmap) {
    color.background <- palette[1]
    color.border <- palette[8]
  }
  
  if (tiny) {
    
    axis.title.x_element <- ggplot2::element_text(size = 8,
                                                  color = color.axis.title,
                                                  vjust = -0.25)
    axis.title.y_element <- ggplot2::element_text(size = 8,
                                                  color = color.axis.title,
                                                  vjust = 1.25)
    axis.text_element <- ggplot2::element_blank()
    plot.margin_element <- grid::unit(c(0, 0, 0.15, 0.15), "cm")
    
  } else {
    
    axis.title.x_element <- ggplot2::element_text(size = 11,
                                                  color = color.axis.title,
                                                  vjust = 0)
    axis.title.y_element <- ggplot2::element_text(size = 11,
                                                  color = color.axis.title,
                                                  vjust = 1.25)
    axis.text_element <- ggplot2::element_text(size = 9,
                                               color = color.axis.text)
    plot.margin_element <- grid::unit(c(0.35, 0.2, 0.3, 0.35), "cm")
  }
  
  ## Begin construction of theme
  theme <- ggplot2::theme_bw(base_size = 9) +
    
    ## Set the entire chart region to a light gray color
    ggplot2::theme(panel.background =
                     ggplot2::element_rect(fill = color.background,
                                           color = color.background)) +
    ggplot2::theme(plot.background =
                     ggplot2::element_rect(fill = color.background,
                                           color = color.background)) +
    ggplot2::theme(panel.border =
                     ggplot2::element_rect(color = color.background)) +
    
    ## Format the grid
    ggplot2::theme(panel.grid.major =
                     ggplot2::element_line(color =
                                             color.grid.major,
                                           size = .25)) +
    ggplot2::theme(panel.grid.minor = ggplot2::element_blank()) +
    ggplot2::theme(axis.ticks = ggplot2::element_blank()) +
    
    ## Format the legend, but hide by default
    ggplot2::theme(legend.position = "none") +
    ggplot2::theme(legend.background =
                     ggplot2::element_rect(fill = color.background)) +
    ggplot2::theme(legend.text =
                     ggplot2::element_text(size = 9,
                                           color = color.axis.title)) +
    
    ## Set title and axis labels, and format these and tick marks
    ggplot2::theme(plot.title = title_element) +
    ggplot2::theme(axis.text = axis.text_element) +
    ggplot2::theme(axis.title.x = axis.title.x_element) +
    ggplot2::theme(axis.title.y = axis.title.y_element) +
    
    ## Format facet headers
    ggplot2::theme(strip.background =
                     ggplot2::element_rect(fill = color.background,
                                           linetype = "blank")) +
    ggplot2::theme(strip.text =
                     ggplot2::element_text(size = 9,
                                           color = color.axis.title,
                                           face = "bold")) +
    
    ## Plot margins
    ggplot2::theme(plot.margin = plot.margin_element)
  
  ## The following relies on this bug in ggplot2:
  ## https://github.com/hadley/ggplot2/issues/892
  if (tiny)
    theme <- theme + ggplot2::theme(axis.ticks.margin =
                                      grid::unit(-0.5, "line"),
                                    axis.ticks.length =
                                      grid::unit(0, "cm"))
  
  if (heatmap)
    theme <- theme +
    ggplot2::theme(panel.grid.major =
                     ggplot2::element_blank()) +
    ggplot2::theme(panel.border =
                     ggplot2::element_rect(color = color.border)) +
    ggplot2::theme(panel.margin = grid::unit(c(0, 0, 0, 0), "cm")) +
    ggplot2::theme(axis.ticks =
                     ggplot2::element_line(color = color.border))
  
  theme
}